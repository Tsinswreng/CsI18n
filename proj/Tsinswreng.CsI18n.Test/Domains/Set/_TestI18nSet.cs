using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Set;

[Doc($$"""
#Sum[{{nameof(I18nSet)}} 的測試類。]
#Descr[
一個測試類對應一個被測類；不同被測成員（`AddLang`、`Use`、`Verify`）
的用例拆到同目錄的其他文件。

`I18nSet` 有可變狀態（當前語言），故**每個用例都自己新建一個集合**，
不共用實例——否則用例之間會互相污染，並行跑更會隨機紅。
]
""")]
public partial class TestI18nSet : ITester {
	[Doc($$"""
	#Sum[把本測試類的用例掛到節點上。]
	#Prm[{{nameof(Node)}}][框架給的節點。]
	#Rtn[掛好用例的節點。]
	""")]
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		// 框架可能不給節點，那就自己造一個。
		Node ??= new TestNode();
		Node.Ordered = false;
		RegisterAddLang(Node);
		RegisterUse(Node);
		RegisterVerify(Node);
		RegisterLangLoader(Node);
		return Node;
	}
}


