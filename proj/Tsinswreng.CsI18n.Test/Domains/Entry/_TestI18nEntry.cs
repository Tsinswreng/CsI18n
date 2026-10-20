using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Entry;

[Doc($$"""
#Sum[{{nameof(I18nEntry)}} 的測試類。]
#Descr[
一個測試類對應一個被測類；不同被測函數的用例拆到同目錄的其他文件。
]
""")]
public partial class TestI18nEntry : ITester {
	[Doc($$"""
	#Sum[把本測試類的用例掛到節點上。]
	#Prm[{{nameof(Node)}}][框架給的節點。]
	#Rtn[掛好用例的節點。]
	""")]
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		// 框架可能不給節點，那就自己造一個。
		Node ??= new TestNode();
		Node.Ordered = false;
		RegisterBuildKeyStr(Node);
		return Node;
	}
}

