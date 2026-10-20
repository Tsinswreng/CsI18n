using Microsoft.Extensions.DependencyInjection;
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
#Sum[測試入口：建容器 → 掛測試樹 → 跑。]
#Descr[
*為何還是建了容器*：本庫的被測物件都是可 new 的純類，
測試類本身不需要注入任何東西；
但 {{nameof(DiEtTestMgr)}} 這代框架要求先 `InitSvc` 才掛測試樹，
故照它的約定走，容器裏不會有多餘東西。

*`ThrowOnFailed: true`*：框架默認只打印失敗、不讓進程失敗。
要用退出碼表達成敗（CI 靠這個），必須要求它在有失敗時拋。
]
""")]
public static class Program {
	[Doc($$"""
	#Sum[非同步主函數。]
	#Prm[{{nameof(Args)}}][命令行參數，本工程目前不用。]
	""")]
	public static async Task Main(str[] Args) {
		IServiceCollection SvcColct = new ServiceCollection();
		// 每個測試類都是無參構造，登記成 Transient 只是讓容器能解析到；
		// 若框架走的是 `new T()` 這條路，這些登記也不會有害。
		SvcColct.AddTransient<TestI18nKey>();
		SvcColct.AddTransient<TestI18nEntry>();
		SvcColct.AddTransient<TestI18nDict>();
		SvcColct.AddTransient<TestIcuFormatter>();
		SvcColct.AddTransient<TestI18nSet>();
		SvcColct.AddTransient<TestI18nJson>();
		SvcColct.AddTransient<TestExampleUsage>();

		var Mgr = I18nTestMgr.Inst;
		_ = Mgr.InitSvc(SvcColct, Colct => Colct.BuildServiceProvider());

		ITestExecutor Executor = new TreeTestExecutor();
		// 參數次序：Root, Arg, Opt, ThrowOnFailed。
		await Executor.RunEtPrint(Mgr.TestNode, NIL, null, true);
	}
}





