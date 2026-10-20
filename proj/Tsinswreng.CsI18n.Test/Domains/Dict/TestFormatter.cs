using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Dict;

// 本文件只測「格式化引擎是可替換的」這條接縫。
public partial class TestI18nDict {
	[Doc($$"""
	#Sum[掛上 {{nameof(I18nDict.Formatter)}} 的用例。]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterFormatter(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestI18nDict),
			[typeof(I18nDict)],
			[nameof(I18nDict.Formatter)],
			"Formatter:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("默認引擎是 ICU", async Arg => {
			// 默認必須是 ICU：複數這類規則才有地方放。
			var Key = I18nKey.Mk(null, ["Files__"]);
			var Dict = I18nDict.FromMap(new Dictionary<str, str> { ["Files__"] = "{0, plural, one {# file} other {# files}}" });
			T(Dict.Formatter is IcuFormatter, $"期望 ICU 引擎，實際 {Dict.Formatter.GetType().Name}");
			var One = Dict.Get(Key, 1);
			var Many = Dict.Get(Key, 3);
			T(One == "1 file", $"實際 {One}");
			T(Many == "3 files", $"實際 {Many}");
			return NIL;
		});
		R("有參數時改用自訂引擎", async Arg => {
			var Key = I18nKey.Mk(null, ["Greet"]);
			var Dict = I18nDict.FromMap(new Dictionary<str, str> { ["Greet"] = "你好，{0}" });
			var Recorder = new RecordingFormatter();
			Dict.Formatter = Recorder;
			var Text = Dict.Get(Key, "小明");
			// 自訂引擎只是把模板包起來，故能證明「有走它」。
			T(Text == "<你好，{0}>", $"期望 <你好，{{0}}>，實際 {Text}");
			T(Recorder.Calls == 1, $"期望調用一次，實際 {Recorder.Calls}");
			T(Recorder.LastTemplate == "你好，{0}", $"期望原模板，實際 {Recorder.LastTemplate}");
			T(Recorder.LastArgs is { Length: 1 }, "引擎應收到位置參數");
			return NIL;
		});

		R("無參數時不驚動自訂引擎", async Arg => {
			// 無參數短路：引擎可能很貴（ICU），也可能不喜歡空參數。
			var Key = I18nKey.Mk(null, ["Greet"]);
			var Dict = I18nDict.FromMap(new Dictionary<str, str> { ["Greet"] = "你好，{0}" });
			var Recorder = new RecordingFormatter();
			Dict.Formatter = Recorder;
			var Text = Dict.Get(Key);
			T(Text == "你好，{0}", $"期望原模板，實際 {Text}");
			T(Recorder.Calls == 0, $"期望不調用，實際調用 {Recorder.Calls} 次");
			return NIL;
		});

		R("未命中時不驚動自訂引擎", async Arg => {
			// 兜底是 OnKeyNotFound 的事，與格式化無關。
			var Dict = I18nDict.FromMap(new Dictionary<str, str>());
			var Recorder = new RecordingFormatter();
			Dict.Formatter = Recorder;
			var Key = I18nKey.Mk(null, ["Missing"]);
			T(Dict.Get(Key, "x") == "Missing");
			T(Recorder.Calls == 0, $"期望不調用，實際調用 {Recorder.Calls} 次");
			return NIL;
		});
	}

	[Doc($$"""
	#Sum[記錄調用次數的假引擎：證明字典確實把活兒交給 {{nameof(I18nDict.Formatter)}}。]
	#Descr[
	故意不實現任何真正的格式化，只把模板包上角括號——
	用例因此不必同時驗「引擎對不對」與「字典有沒有用它」。
	]
	""")]
	sealed class RecordingFormatter : IFormatter {
		[Doc("被調用幾次。")]
		public i32 Calls { get; set; }

		[Doc("最後一次收到的模板。")]
		public str? LastTemplate { get; set; }

		[Doc("最後一次收到的參數。")]
		public obj?[]? LastArgs { get; set; }

		[Doc($$"""
		#Sum[把模板包上角括號，並記下調用。]
		#Prm[{{nameof(Template)}}][模板。]
		#Prm[{{nameof(Args)}}][位置參數。]
		#Rtn[包好的文字。]
		""")]
		public str Format(str Template, obj?[] Args) {
			Calls++;
			LastTemplate = Template;
			LastArgs = Args;
			return "<" + Template + ">";
		}
	}
}


