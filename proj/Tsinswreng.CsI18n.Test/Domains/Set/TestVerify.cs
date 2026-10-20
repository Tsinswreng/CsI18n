using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsI18n.Test.Utils;

namespace Tsinswreng.CsI18n.Test.Domains.Set;

// 本文件只測「覆蓋率自檢」這一個函數。
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
			var Login = I18nKey.Mk(null, ["Login"]);
			var Set = new I18nSet()
				.AddLang("zh-CN", [new I18nEntry(Login, "登錄")])
				.AddLang("en", [new I18nEntry(Login, "Logged in")]);
			var Problems = Set.Verify([Login]);
			T(Problems.Count == 0, $"期望零問題，實際 {Problems.Count}：{str.Join(" | ", Problems)}");
			return NIL;
		});

		R("缺譯：指出語言與鍵", async Arg => {
			var Login = I18nKey.Mk(null, ["Login"]);
			var Logout = I18nKey.Mk(null, ["Logout"]);
			var Set = new I18nSet()
				.AddLang("zh-CN", [
					new I18nEntry(Login, "登錄"),
					new I18nEntry(Logout, "退出登錄"),
				])
				.AddLang("en", [new I18nEntry(Login, "Logged in")]);
			var Problems = Set.Verify([Login, Logout]);
			T(Problems.Count == 1, $"期望一個問題，實際 {Problems.Count}：{str.Join(" | ", Problems)}");
			// 只有 en 缺 Logout；信息要能直接定位到「哪個語言的哪個鍵」。
			T(Problems[0].Contains("[en]"), $"信息應帶語言：{Problems[0]}");
			T(Problems[0].Contains("缺少文案"), $"信息應說明是缺譯：{Problems[0]}");
			T(Problems[0].Contains("Logout"), $"信息應帶鍵名：{Problems[0]}");
			return NIL;
		});

		R("空譯文也算問題", async Arg => {
			// 翻成空白的鍵在界面上等於沒翻，故不能只看「鍵在不在」。
			var Login = I18nKey.Mk(null, ["Login"]);
			var Set = new I18nSet()
				.AddLang("zh-CN", [new I18nEntry(Login, "登錄")])
				.AddLang("en", [new I18nEntry(Login, "   ")]);
			var Problems = Set.Verify([Login]);
			T(Problems.Count == 1, $"期望一個問題，實際 {Problems.Count}");
			T(Problems[0].Contains("譯文為空"), $"信息應說明是空譯文：{Problems[0]}");
			return NIL;
		});

		R("多出來的鍵不算問題", async Arg => {
			// 允許字典比必需鍵多：例如錯誤文案不必列入界面文案的必需鍵。
			var Login = I18nKey.Mk(null, ["Login"]);
			var Extra = I18nKey.Mk(null, ["NotRequired"]);
			var Set = new I18nSet().AddLang("zh-CN", [
				new I18nEntry(Login, "登錄"),
				new I18nEntry(Extra, "多餘的"),
			]);
			var Problems = Set.Verify([Login]);
			T(Problems.Count == 0, $"期望零問題，實際 {Problems.Count}：{str.Join(" | ", Problems)}");
			return NIL;
		});

		R("可一次檢查多組必需鍵", async Arg => {
			// 界面文案一組、錯誤文案一組，兩組都要覆蓋到。
			var Login = I18nKey.Mk(null, ["Login"]);
			var ErrKey = I18nKey.Mk(null, ["Error", "NotLogin"]);
			var Set = new I18nSet().AddLang("zh-CN", [new I18nEntry(Login, "登錄")]);
			var Problems = Set.Verify([Login], [ErrKey]);
			T(Problems.Count == 1, $"期望一個問題，實際 {Problems.Count}");
			T(Problems[0].Contains("缺少文案"), $"應報缺譯：{Problems[0]}");
			T(Problems[0].Contains("Error/NotLogin"), $"應報到錯誤鍵：{Problems[0]}");
			return NIL;
		});

		R("同一鍵出現在多組裏只報一次", async Arg => {
			// 必需鍵會被去重；重複登記不該讓同一個漏譯刷出多行。
			var Login = I18nKey.Mk(null, ["Login"]);
			var Set = new I18nSet().AddLang("en", new Dictionary<str, str>());
			var Problems = Set.Verify([Login], [Login]);
			T(Problems.Count == 1, $"期望一個問題，實際 {Problems.Count}");
			return NIL;
		});

		R("鍵清單裏有 null：直接拋", async Arg => {
			// null 是代碼錯（多半是鍵類的靜態初始化成了循環），
			// 放過去的話只會變成一行沒有信息量的「缺少文案：」。
			var Set = new I18nSet().AddLang("en", new Dictionary<str, str>());
			// 顯式宣告成鍵陣列：若直接寫 `Verify([null])`，
			// 編譯器會把它當成「一組本身是 null」，測到的就不是這條守衛了。
			IKeyNode?[] Group = [null];
			T(Chk.Throws<ArgumentException>(() => Set.Verify(Group!)));
			return NIL;
		});

		R("必需鍵組本身是 null：直接拋", async Arg => {
			// 少寫一組鍵時很容易傳成 Verify(GroupA, null)；
			// 這也屬代碼錯，不該以 NullReferenceException 的形式炸出來。
			var Login = I18nKey.Mk(null, ["Login"]);
			var Set = new I18nSet().AddLang("en", new Dictionary<str, str>());
			IEnumerable<IKeyNode> Good = [Login];
			IEnumerable<IKeyNode>[] Groups = [Good, null!];
			T(Chk.Throws<ArgumentException>(() => Set.Verify(Groups)));
			return NIL;
		});

		R("必需鍵組清單本身是 null：直接拋", async Arg => {
			// 上游變數忘了賦值時會走到這裏。
			var Set = new I18nSet().AddLang("en", new Dictionary<str, str>());
			T(Chk.Throws<ArgumentException>(() => Set.Verify(null!)));
			return NIL;
		});

		R("鍵的路徑為空：直接拋", async Arg => {
			// 路徑為空的鍵在字典裏就是空字符串鍵，查得到也翻不到，屬代碼錯。
			var EmptyPath = I18nKey.Mk(null, []);
			var Set = new I18nSet().AddLang("en", new Dictionary<str, str>());
			T(Chk.Throws<ArgumentException>(() => Set.Verify([EmptyPath])));
			return NIL;
		});
	}
}
