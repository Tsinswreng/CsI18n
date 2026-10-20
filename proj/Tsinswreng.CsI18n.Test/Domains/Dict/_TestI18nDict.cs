using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Dict;

[Doc($$"""
#Sum[{{nameof(I18nDict)}} 的測試類。]
#Descr[
一個測試類對應一個被測類；不同被測成員（`Get`、`Mk`、`FromMap`、`Formatter`）
的用例拆到同目錄的其他文件。
]
""")]
public partial class TestI18nDict : ITester {
	[Doc($$"""
	#Sum[把本測試類的用例掛到節點上。]
	#Prm[{{nameof(Node)}}][框架給的節點。]
	#Rtn[掛好用例的節點。]
	""")]
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		// 框架可能不給節點，那就自己造一個。
		Node ??= new TestNode();
		Node.Ordered = false;
		RegisterGet(Node);
		RegisterMk(Node);
		RegisterFromMap(Node);
		RegisterFormatter(Node);
		return Node;
	}
}

