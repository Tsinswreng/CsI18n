using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsI18n.Test.Domains.Dict;
using Tsinswreng.CsI18n.Test.Domains.Entry;
using Tsinswreng.CsI18n.Test.Domains.ExampleUsage;
using Tsinswreng.CsI18n.Test.Domains.Formatter.Icu;
using Tsinswreng.CsI18n.Test.Domains.Json;
using Tsinswreng.CsI18n.Test.Domains.Key;
using Tsinswreng.CsI18n.Test.Domains.Set;

namespace Tsinswreng.CsI18n.Test;

[Doc($$"""
#Sum[本測試工程的測試管理器：收編全部測試類。]
#Descr[
每個測試 csproj 都要有一個 {{nameof(DiEtTestMgr)}}；
它只負責「登記哪些測試類」，用例本身在各自的測試類裏。

*為甚麼一個 tester 對一個被測類*：一個被測類的行為放一起看，
用例按被測函數再拆文件（見同目錄下各 `Domains/*`），
出錯時從路徑就能看出是哪個類的哪個函數壞了。
]
""")]
public class I18nTestMgr : DiEtTestMgr {
	[Doc("單例；入口直接取它。")]
	public static I18nTestMgr Inst = new();

	[Doc($$"""
	#Sum[把各測試類掛到根節點下。]
	#Prm[{{nameof(Node)}}][上層給的節點；本管理器固定用自己的根節點。]
	#Rtn[含全部測試類的根節點。]
	""")]
	public override ITestNode RegisterTestsInto(ITestNode? Node) {
		Node = this.TestNode;
		this.RegisterTester<TestI18nKey>();
		this.RegisterTester<TestI18nEntry>();
		this.RegisterTester<TestI18nDict>();
		this.RegisterTester<TestIcuFormatter>();
		this.RegisterTester<TestI18nSet>();
		this.RegisterTester<TestI18nJson>();
		this.RegisterTester<TestExampleUsage>();
		return Node;
	}
}






