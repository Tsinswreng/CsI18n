using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Entry;

// 本文件只測「鍵 → 字典鍵」這一步（含由它決定的構造器行為）。
public partial class TestI18nEntry {
	[Doc($$"""
	#Sum[掛上 {{nameof(I18nEntry.BuildKeyStr)}} 的用例。]
	#Descr[
	構造器的全部職責就是算這個鍵字符串，故構造器的斷言也放這裏。
	]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterBuildKeyStr(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestI18nEntry),
			[typeof(I18nEntry)],
			[nameof(I18nEntry.BuildKeyStr)],
			"BuildKeyStr:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("單段鍵：字典鍵就是鍵名", async Arg => {
			var Key = I18nKey.Mk(null, ["Login"]);
			var KeyStr = I18nEntry.BuildKeyStr(Key);
			T(KeyStr == "Login", $"期望 Login，實際 {KeyStr}");
			return NIL;
		});

		R("嵌套鍵：字典鍵父先子後", async Arg => {
			var Parent = I18nKey.Mk(null, ["View"]);
			var Child = I18nKey.Mk(Parent, ["Login"]);
			var KeyStr = I18nEntry.BuildKeyStr(Child);
			T(KeyStr == "View/Login", $"期望 View/Login，實際 {KeyStr}");
			return NIL;
		});

		R("分隔符固定用斜線，不隨鍵自己的 PathSep", async Arg => {
			// 同一個字典要同時容納「平鋪的界面文案鍵」與「樹狀的錯誤鍵」，
			// 若沿用各節點自己的 PathSep，同一條譯文就會有兩種寫法。
			var Key = new I18nKey {
				RelaPathSegs = ["View", "Login"],
				PathSep = ".",
			};
			T(Key.BuildFullPath() == "View.Login", "前置條件：鍵自己的分隔符是點");
			var KeyStr = I18nEntry.BuildKeyStr(Key);
			T(KeyStr == "View/Login", $"期望 View/Login，實際 {KeyStr}");
			return NIL;
		});

		R("構造條目時算出字典鍵", async Arg => {
			var Key = I18nKey.Mk(null, ["Login"]);
			var Entry = new I18nEntry(Key, "登錄");
			T(Entry.Key == Key, "條目應保留鍵物件本身，便於回溯");
			T(Entry.KeyStr == "Login", $"期望 Login，實際 {Entry.KeyStr}");
			T(Entry.Text == "登錄", $"期望 登錄，實際 {Entry.Text}");
			return NIL;
		});

		R("模板原樣保留：大括號與換行都不動", async Arg => {
			// 模板是給格式化引擎用的，條目這層不該碰它。
			var Template = "第一行 {0}\n第二行";
			var Entry = new I18nEntry(I18nKey.Mk(null, ["Multi"]), Template);
			T(Entry.Text == Template);
			T(Entry.KeyStr == "Multi");
			return NIL;
		});
	}
}

