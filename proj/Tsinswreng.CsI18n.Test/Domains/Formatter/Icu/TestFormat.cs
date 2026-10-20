using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Formatter.Icu;

// 本文件只測 ICU 引擎「填參數」這一個函數。
public partial class TestIcuFormatter {
	[Doc($$"""
	#Sum[掛上 {{nameof(IcuFormatter.Format)}} 的用例。]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterFormat(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestIcuFormatter),
			[typeof(IcuFormatter)],
			[nameof(IcuFormatter.Format)],
			"Format:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;
		var F = IcuFormatter.Inst;

		R("位置參數照常可用", async Arg => {
			// ICU 的參數本來是具名的；本庫把位置參數轉成 "0"、"1"…，故 `{0}` 寫法不變。
			var Text = F.Format("歡迎，{0}", ["小明"]);
			T(Text == "歡迎，小明", $"實際 {Text}");
			var Two = F.Format("{0}-{1}", ["A", "B"]);
			T(Two == "A-B", $"實際 {Two}");
			return NIL;
		});

		R("複數：按數量選分支", async Arg => {
			// 這才是默認用 ICU 的理由：英文 one/other 這類規則由引擎管，不寫在代碼裏。
			const str Template = "{0, plural, one {# file} other {# files}}";
			var One = F.Format(Template, [1]);
			var Many = F.Format(Template, [3]);
			T(One == "1 file", $"期望 1 file，實際 {One}");
			T(Many == "3 files", $"期望 3 files，實際 {Many}");
			return NIL;
		});

		R("無參數：原樣返回", async Arg => {
			var Text = F.Format("登錄", []);
			T(Text == "登錄", $"實際 {Text}");
			return NIL;
		});

		R("括號沒配對：退回原文而不是拋", async Arg => {
			var Text = F.Format("壞 {0", [1]);
			T(Text == "壞 {0", $"期望原文，實際 {Text}");
			return NIL;
		});
	}
}

