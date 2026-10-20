using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Dict;

// 本文件只測「按鍵取文案」這一個函數（含索引器，它只是 Get 的無參數形式）。
public partial class TestI18nDict {
	[Doc($$"""
	#Sum[掛上 {{nameof(I18nDict.Get)}} 的用例。]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterGet(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestI18nDict),
			[typeof(I18nDict)],
			[nameof(I18nDict.Get)],
			"Get:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("命中且無參數：原樣返回模板", async Arg => {
			var Key = I18nKey.Mk(null, ["Login"]);
			var Dict = I18nDict.Mk([new I18nEntry(Key, "登錄")]);
			var Text = Dict.Get(Key);
			T(Text == "登錄", $"期望 登錄，實際 {Text}");
			return NIL;
		});

		R("模板含佔位符但沒傳參數時不填", async Arg => {
			// 無參數短路是刻意的：讓自訂格式化引擎不必處理空參數，
			// 也讓「就想原樣顯示帶大括號的文案」這種情況成立。
			var Key = I18nKey.Mk(null, ["Raw"]);
			var Dict = I18nDict.Mk([new I18nEntry(Key, "a{0}b")]);
			var Text = Dict.Get(Key);
			T(Text == "a{0}b", $"期望 a{{0}}b，實際 {Text}");
			return NIL;
		});

		R("有參數時按位置填入", async Arg => {
			var Key = I18nKey.Mk(null, ["Greet"]);
			var Dict = I18nDict.Mk([new I18nEntry(Key, "你好，{0}")]);
			var Text = Dict.Get(Key, "小明");
			T(Text == "你好，小明", $"期望 你好，小明，實際 {Text}");
			return NIL;
		});

		R("多個位置參數按次序填入", async Arg => {
			var Key = I18nKey.Mk(null, ["Range"]);
			var Dict = I18nDict.Mk([new I18nEntry(Key, "從 {0} 到 {1}")]);
			var Text = Dict.Get(Key, "一月", "三月");
			T(Text == "從 一月 到 三月", $"實際 {Text}");
			return NIL;
		});

		R("未命中：默認回鍵的全路徑", async Arg => {
			// 漏翻的鍵要能在界面上直接看出來，而不是顯示空白。
			var Dict = I18nDict.Mk([new I18nEntry(I18nKey.Mk(null, ["Login"]), "登錄")]);
			var Missing = I18nKey.Mk(null, ["Missing"]);
			var Text = Dict.Get(Missing);
			T(Text == "Missing", $"期望 Missing，實際 {Text}");
			return NIL;
		});

		R("未命中的嵌套鍵：回父先子後的全路徑", async Arg => {
			var Dict = I18nDict.Mk([new I18nEntry(I18nKey.Mk(null, ["Login"]), "登錄")]);
			var Parent = I18nKey.Mk(null, ["View"]);
			var Missing = I18nKey.Mk(Parent, ["Nope"]);
			var Text = Dict.Get(Missing);
			T(Text == "View/Nope", $"期望 View/Nope，實際 {Text}");
			return NIL;
		});

		R("可換掉未命中的兜底", async Arg => {
			var Dict = I18nDict.Mk([new I18nEntry(I18nKey.Mk(null, ["Login"]), "登錄")]);
			var Missing = I18nKey.Mk(null, ["Missing"]);
			IKeyNode? SeenKey = null;
			obj?[]? SeenArgs = null;
			Dict.OnKeyNotFound = (Self, Key, Args) => {
				SeenKey = Key;
				SeenArgs = Args;
				return "自訂兜底";
			};
			var Text = Dict.Get(Missing, "x");
			T(Text == "自訂兜底", $"期望 自訂兜底，實際 {Text}");
			// 兜底要能拿到「是哪個鍵、帶了什麼參數」，否則日誌裏查不出漏翻的位置。
			T(SeenKey == Missing, "兜底應收到原鍵");
			T(SeenArgs is { Length: 1 } && SeenArgs[0] is "x", "兜底應收到位置參數");
			return NIL;
		});

		R("索引器等價於無參數 Get", async Arg => {
			var Key = I18nKey.Mk(null, ["Login"]);
			var Dict = I18nDict.Mk([new I18nEntry(Key, "登錄")]);
			T(Dict[Key] == Dict.Get(Key));
			T(Dict[Key] == "登錄");
			return NIL;
		});
	}
}
