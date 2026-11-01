using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Set;

// 本文件只測「覆蓋率自檢」這一個函數。
// 自檢不收任何鍵清單：必檢鍵就是所有語言已提供鍵的聯集，故用例全部用字典直接鋪資料。
public partial class TestI18nSet {
	[Doc($$"""
	#Sum[掛上 {{nameof(I18nSet.Verify)}} 的用例。]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterVerify(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestI18nSet),
			[typeof(I18nSet)],
			[nameof(I18nSet.Verify)],
			"Verify:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("每個語言都翻齊：零問題", async Arg => {
			var Set = new I18nSet()
				.AddLang("zh-Hant", new Dictionary<str, str> { ["Login"] = "登錄" })
				.AddLang("en", new Dictionary<str, str> { ["Login"] = "Logged in" });
			var Problems = Set.Verify();
			T(Problems.Count == 0, $"期望零問題，實際 {Problems.Count}：{str.Join(" | ", Problems)}");
			return NIL;
		});

		R("缺譯：指出語言與鍵", async Arg => {
			// 聯集是 {Login, Logout}；en 少了 Logout，這就是缺譯。
			var Set = new I18nSet()
				.AddLang("zh-Hant", new Dictionary<str, str> {
					["Login"] = "登錄",
					["Logout"] = "退出登錄",
				})
				.AddLang("en", new Dictionary<str, str> { ["Login"] = "Logged in" });
			var Problems = Set.Verify();
			T(Problems.Count == 1, $"期望一個問題，實際 {Problems.Count}：{str.Join(" | ", Problems)}");
			// 信息要能直接定位到「哪個語言的哪個鍵」。
			T(Problems[0].Contains("[en]"), $"信息應帶語言：{Problems[0]}");
			T(Problems[0].Contains("missing translation"), $"信息應說明是缺譯：{Problems[0]}");
			T(Problems[0].Contains("Logout"), $"信息應帶鍵名：{Problems[0]}");
			return NIL;
		});

		R("空譯文也算問題", async Arg => {
			// 翻成空白的鍵在界面上等於沒翻，故不能只看「鍵在不在」。
			var Set = new I18nSet()
				.AddLang("zh-Hant", new Dictionary<str, str> { ["Login"] = "登錄" })
				.AddLang("en", new Dictionary<str, str> { ["Login"] = "   " });
			var Problems = Set.Verify();
			T(Problems.Count == 1, $"期望一個問題，實際 {Problems.Count}");
			T(Problems[0].Contains("empty translation"), $"信息應說明是空譯文：{Problems[0]}");
			return NIL;
		});

		R("只有一個語言時恆為自洽", async Arg => {
			// 聯集完全由這個語言自己決定，故它不可能缺譯；沒有必需鍵清單，也就沒有「多出來的鍵」。
			var Set = new I18nSet().AddLang("zh-Hant", new Dictionary<str, str> {
				["Login"] = "登錄",
				["NotUsedAnywhere"] = "只有這一個語言有",
			});
			var Problems = Set.Verify();
			T(Problems.Count == 0, $"期望零問題，實際 {Problems.Count}：{str.Join(" | ", Problems)}");
			return NIL;
		});

		R("多個語言都缺同一鍵：逐語言各報一次", async Arg => {
			// 缺譯是按語言報的：兩個語言都缺 Logout，就該有兩條，各自的語言代碼不同。
			var Set = new I18nSet()
				.AddLang("zh-Hant", new Dictionary<str, str> {
					["Login"] = "登錄",
					["Logout"] = "退出登錄",
				})
				.AddLang("en", new Dictionary<str, str> { ["Login"] = "Logged in" })
				.AddLang("ja", new Dictionary<str, str> { ["Login"] = "ログイン" });
			var Problems = Set.Verify();
			var Missing = Problems.Where(x => x.Contains("Logout")).ToList();
			T(Missing.Count == 2, $"期望兩個語言各一條，實際 {Missing.Count}：{str.Join(" | ", Problems)}");
			T(Missing.Any(x => x.Contains("[en]")), $"應有 en 一條：{str.Join(" | ", Missing)}");
			T(Missing.Any(x => x.Contains("[ja]")), $"應有 ja 一條：{str.Join(" | ", Missing)}");
			return NIL;
		});

		R("沒有任何語言：報一條而不是假裝通過", async Arg => {
			// 空清單若回零問題，呼叫方會以為檢查通過了；這比報錯更壞。
			var Set = new I18nSet();
			var Problems = Set.Verify();
			T(Problems.Count == 1, $"期望一條問題，實際 {Problems.Count}");
			T(Problems[0].Contains("No language"), $"應說明沒有任何語言：{Problems[0]}");
			return NIL;
		});

		R("一次報完：不因第一條缺譯就停", async Arg => {
			// 三種語言、兩處缺譯、一處空譯文，應一次全部交出，不必修一條跑一次。
			var Set = new I18nSet()
				.AddLang("zh-Hant", new Dictionary<str, str> {
					["A"] = "甲",
					["B"] = "乙",
					["C"] = "丙",
				})
				.AddLang("en", new Dictionary<str, str> {
					["A"] = "A",
					["B"] = "",
				})
				.AddLang("ja", new Dictionary<str, str> {
					["A"] = "A",
					["B"] = "B",
				});
			var Problems = Set.Verify();
			// en 缺 C；ja 缺 C；en 的 B 是空譯文 ⇒ 共三條。
			T(Problems.Count == 3, $"期望三條問題，實際 {Problems.Count}：{str.Join(" | ", Problems)}");
			return NIL;
		});
	}
}
