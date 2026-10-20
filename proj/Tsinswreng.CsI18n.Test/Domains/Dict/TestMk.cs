using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsI18n.Test.Utils;

namespace Tsinswreng.CsI18n.Test.Domains.Dict;

// 本文件只測「由譯文條目建字典」。
public partial class TestI18nDict {
	[Doc($$"""
	#Sum[掛上 {{nameof(I18nDict.Mk)}} 的用例。]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterMk(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestI18nDict),
			[typeof(I18nDict)],
			[nameof(I18nDict.Mk)],
			"Mk:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("條目全部進字典", async Arg => {
			var Login = I18nKey.Mk(null, ["Login"]);
			var Logout = I18nKey.Mk(null, ["Logout"]);
			var Dict = I18nDict.Mk([
				new I18nEntry(Login, "登錄"),
				new I18nEntry(Logout, "退出登錄"),
			]);
			T(Dict.Map.Count == 2, $"期望兩條，實際 {Dict.Map.Count}");
			T(Dict.Get(Login) == "登錄");
			T(Dict.Get(Logout) == "退出登錄");
			return NIL;
		});

		R("空條目集：空字典，任何鍵都走兜底", async Arg => {
			var Dict = I18nDict.Mk([]);
			T(Dict.Map.Count == 0);
			var Key = I18nKey.Mk(null, ["Login"]);
			T(Dict.Get(Key) == "Login");
			return NIL;
		});

		R("同一鍵重複出現：建字典時就拋", async Arg => {
			// 重複鍵一定是複製貼上沒刪；放過去也只會靜默用其中一條，
			// 不如在啟動時就炸出來。
			var Key = I18nKey.Mk(null, ["Login"]);
			T(Chk.Throws<InvalidOperationException>(() => I18nDict.Mk([
				new I18nEntry(Key, "登錄"),
				new I18nEntry(Key, "登入"),
			])));
			return NIL;
		});
	}
}
