using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Key;

// 本文件只測 I18nKey.Mk 造出來的鍵在路徑上的行為；
// 鍵的「路徑推導」本身屬 CsKeyNode，此處只驗本庫把它當鍵用的那部分。
public partial class TestI18nKey {
	[Doc($$"""
	#Sum[掛上 {{nameof(I18nKey.Mk)}} 的用例。]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterMk(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestI18nKey), // tester type
			[typeof(I18nKey)], // testee types
			[nameof(I18nKey.Mk)], // testee fn names
			"Mk:" // name prefix
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("單段鍵的全路徑就是鍵名", async Arg => {
			var Key = I18nKey.Mk(null, ["Login"]);
			var FullPath = Key.BuildFullPath();
			T(FullPath == "Login", $"期望 Login，實際 {FullPath}");
			T(Key.ToString() == "Login", $"期望 ToString 為 Login，實際 {Key.ToString()}");
			return NIL;
		});

		R("父子兩段鍵沿父鏈拼成全路徑", async Arg => {
			var Parent = I18nKey.Mk(null, ["View"]);
			var Child = I18nKey.Mk(Parent, ["Login"]);
			// 父先子後：上溯得到的段序若反了，鍵就會變成 Login/View。
			var Segs = Child.BuildFullPathSegs();
			T(Segs.Count == 2, $"期望兩段，實際 {Segs.Count}");
			T(Segs[0] == "View" && Segs[1] == "Login", $"期望 View,Login，實際 {str.Join(",", Segs)}");
			T(Child.BuildFullPath() == "View/Login", $"期望 View/Login，實際 {Child.BuildFullPath()}");
			return NIL;
		});

		R("可另給分隔符拼路徑", async Arg => {
			var Parent = I18nKey.Mk(null, ["View"]);
			var Child = I18nKey.Mk(Parent, ["Login"]);
			T(Child.BuildFullPath(".") == "View.Login");
			return NIL;
		});

		R("文案鍵可當通用鍵節點用", async Arg => {
			// 字典只認 IKeyNode；錯誤鍵（IErrNode）、配置鍵也都走這條路，
			// 故文案鍵必須能直接當 IKeyNode 用。
			IKeyNode AsNode = I18nKey.Mk(null, ["Login"]);
			T(AsNode.RelaPathSegs.Count == 1);
			T(AsNode.BuildFullPath() == "Login");
			return NIL;
		});
	}
}
