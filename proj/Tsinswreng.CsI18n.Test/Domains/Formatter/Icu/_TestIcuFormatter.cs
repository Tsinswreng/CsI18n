using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Formatter.Icu;

[Doc($$"""
#Sum[{{nameof(IcuFormatter)}} 的測試類。]
#Descr[
默認引擎：ICU MessageFormat。位置參數照舊，重點是複數與樣式，
以及「模板寫錯不該炸界面」這條約定。
]
""")]
public partial class TestIcuFormatter : ITester {
	[Doc($$"""
	#Sum[把本測試類的用例掛到節點上。]
	#Prm[{{nameof(Node)}}][框架給的節點。]
	#Rtn[掛好用例的節點。]
	""")]
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		// 框架可能不給節點，那就自己造一個。
		Node ??= new TestNode();
		Node.Ordered = false;
		RegisterFormat(Node);
		return Node;
	}
}
