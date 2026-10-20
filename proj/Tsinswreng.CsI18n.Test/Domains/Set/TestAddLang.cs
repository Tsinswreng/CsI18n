using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Set;

// 本文件只測「加入語言」這一個函數（鍵 → 模板字典）。
public partial class TestI18nSet {
	[Doc($$"""
	#Sum[掛上 {{nameof(I18nSet.AddLang)}} 的用例。]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterAddLang(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestI18nSet),
			[typeof(I18nSet)],
			[nameof(I18nSet.AddLang)],
			"AddLang:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("第一個語言自動成為默認語言", async Arg => {
			var Key = I18nKey.Mk(null, ["Login"]);
			var Set = new I18nSet().AddLang("zh-CN", new Dictionary<str, str> { ["Login"] = "登錄" });
			T(Set.DfltLang == "zh-CN", $"期望 zh-CN，實際 {Set.DfltLang}");
			T(Set.Cur.Get(Key) == "登錄");
			return NIL;
		});

		R("再加語言不改默認", async Arg => {
			// 默認語言決定「啟動時顯示什麼」，不該被後來加入的語言搶走。
			var Key = I18nKey.Mk(null, ["Login"]);
			var Set = new I18nSet()
				.AddLang("zh-CN", new Dictionary<str, str> { ["Login"] = "登錄" })
				.AddLang("en", new Dictionary<str, str> { ["Login"] = "Logged in" });
			T(Set.DfltLang == "zh-CN", $"期望 zh-CN，實際 {Set.DfltLang}");
			T(Set.Cur.Get(Key) == "登錄");
			return NIL;
		});

		R("同一語言重複加入：後者覆蓋前者", async Arg => {
			// 語言包分檔載入、後載入的想覆蓋前一批時用得到。
			var Key = I18nKey.Mk(null, ["Login"]);
			var Set = new I18nSet()
				.AddLang("zh-CN", new Dictionary<str, str> { ["Login"] = "登錄" })
				.AddLang("zh-CN", new Dictionary<str, str> { ["Login"] = "登入" });
			T(Set.Dicts.Count == 1, $"期望只剩一個語言，實際 {Set.Dicts.Count}");
			T(Set.Cur.Get(Key) == "登入", $"期望 登入，實際 {Set.Cur.Get(Key)}");
			return NIL;
		});

		R("由鍵→模板字典加入（外部語言包那條路）", async Arg => {
			var Key = I18nKey.Mk(null, ["Login"]);
			var Set = new I18nSet().AddLang("en", new Dictionary<str, str> {
				[Key.BuildFullPath(I18nDict.KeySep)] = "Logged in",
			});
			T(Set.DfltLang == "en");
			T(Set.Cur.Get(Key) == "Logged in");
			return NIL;
		});
	}
}
