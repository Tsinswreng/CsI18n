using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Json;

[Doc($$"""
#Sum[{{nameof(I18nJson)}} 的測試類。]
#Descr[
本庫唯一碰 IO 的地方；攤平規則是「嵌套路徑段用 `/` 連接」，
必須與 {{nameof(I18nEntry.BuildKeyStr)}} 一致，否則 JSON 語言包與 C# 寫的譯文對不上。
]
""")]
public partial class TestI18nJson : ITester {
	[Doc($$"""
	#Sum[把本測試類的用例掛到節點上。]
	#Prm[{{nameof(Node)}}][框架給的節點。]
	#Rtn[掛好用例的節點。]
	""")]
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		// 框架可能不給節點，那就自己造一個。
		Node ??= new TestNode();
		Node.Ordered = false;
		RegisterFromJson(Node);
		RegisterLoadFile(Node);
		RegisterToJson(Node);
		return Node;
	}
}


