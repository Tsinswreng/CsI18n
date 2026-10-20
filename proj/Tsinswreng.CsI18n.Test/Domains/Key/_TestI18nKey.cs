using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Key;

[Doc($$"""
#Sum[{{nameof(I18nKey)}} 的測試類。]
#Descr[
一個測試類對應一個被測類；不同被測函數的用例拆到同目錄的其他文件，
本文件只負責組裝。

被測物件都是純類（無 IO、無狀態共享），故用例可並行（`Ordered` 保持 false）。
]
""")]
public partial class TestI18nKey : ITester {
	[Doc($$"""
	#Sum[把本測試類的用例掛到節點上。]
	#Prm[{{nameof(Node)}}][框架給的節點。]
	#Rtn[掛好用例的節點。]
	""")]
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		// 框架可能不給節點，那就自己造一個。
		Node ??= new TestNode();
		Node.Ordered = false;
		RegisterMk(Node);
		return Node;
	}
}

