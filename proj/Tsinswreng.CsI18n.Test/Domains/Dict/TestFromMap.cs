using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Dict;

// 本文件只測「由鍵→模板字典建字典」（外部語言包那條路）。
public partial class TestI18nDict {
	[Doc($$"""
	#Sum[掛上 {{nameof(I18nDict.FromMap)}} 的用例。]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterFromMap(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestI18nDict),
			[typeof(I18nDict)],
			[nameof(I18nDict.FromMap)],
			"FromMap:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("字典的鍵值照搬", async Arg => {
			var Dict = I18nDict.FromMap(new Dictionary<str, str> {
				["Login"] = "登錄",
				["View/Logout"] = "退出登錄",
			});
			T(Dict.Get(I18nKey.Mk(null, ["Login"])) == "登錄");
			var Parent = I18nKey.Mk(null, ["View"]);
			T(Dict.Get(I18nKey.Mk(Parent, ["Logout"])) == "退出登錄");
			return NIL;
		});

		R("空字典：任何鍵都走兜底", async Arg => {
			var Dict = I18nDict.FromMap(new Dictionary<str, str>());
			T(Dict.Map.Count == 0);
			T(Dict.Get(I18nKey.Mk(null, ["Login"])) == "Login");
			return NIL;
		});

		R("模板照搬，不在這時格式化", async Arg => {
			var Dict = I18nDict.FromMap(new Dictionary<str, str> {
				["Greet"] = "你好，{0}",
			});
			var Key = I18nKey.Mk(null, ["Greet"]);
			T(Dict.Get(Key) == "你好，{0}");
			T(Dict.Get(Key, "小明") == "你好，小明");
			return NIL;
		});
	}
}
