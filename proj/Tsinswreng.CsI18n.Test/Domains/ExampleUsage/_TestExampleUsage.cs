using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.ExampleUsage;

[Doc($$"""
#Sum[綜合示例：一條用例走完本庫的典型用法。]
#Descr[
這一個測試類是給人讀的：用例不分片，從頭讀到尾，
每一步（取文案 → 切語言 → 帶參數 → 錯誤鍵 → 漏翻兜底 → 覆蓋率自檢 → 併入外部 JSON）
都配一段說明。

示例應用擺在 `ExampleApp.cs`：{{nameof(KeysI18n)}}（界面文案鍵）、{{nameof(KeysErr)}}（錯誤鍵）、
{{nameof(MsgZhCn)}}／{{nameof(MsgEn)}}（譯文）、{{nameof(AppI18n)}}（門面）。

用例作用在靜態的 {{nameof(AppI18n)}} 上（語言是共享狀態），故本節點順序執行；
用例內部也會在需要時切回默認語言。
]
""")]
public partial class TestExampleUsage : ITester {
	[Doc($$"""
	#Sum[把綜合示例掛到節點上。]
	#Prm[{{nameof(Node)}}][框架給的節點。]
	#Rtn[掛好用例的節點。]
	""")]
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		// 框架可能不給節點，那就自己造一個。
		Node ??= new TestNode();
		// 語言是共享狀態，同一節點下來的用例不要並行。
		Node.Ordered = true;
		RegisterUsage(Node);
		return Node;
	}
}
