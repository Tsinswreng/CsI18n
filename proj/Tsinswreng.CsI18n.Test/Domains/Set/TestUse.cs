using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsI18n.Test.Utils;

namespace Tsinswreng.CsI18n.Test.Domains.Set;

// 本文件只測「切換／取用當前語言」這條線（Use、Cur、LangCodes）。
public partial class TestI18nSet {
	[Doc($$"""
	#Sum[掛上 {{nameof(I18nSet.Use)}} 的用例。]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterUse(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestI18nSet),
			[typeof(I18nSet)],
			[nameof(I18nSet.Use)],
			"Use:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("一個語言都沒有時取當前語言：拋", async Arg => {
			// 沒語言可用是配置錯，不該回一句空文案糊過去。
			var Set = new I18nSet();
			T(Chk.Throws<InvalidOperationException>(() => { _ = Set.Cur; }));
			return NIL;
		});

		R("首次取當前語言即切到默認語言", async Arg => {
			var Key = I18nKey.Mk(null, ["Login"]);
			var Set = new I18nSet()
				.AddLang("zh-CN", [new I18nEntry(Key, "登錄")])
				.AddLang("en", [new I18nEntry(Key, "Logged in")]);
			// 還沒 Use 過，故 Cur 應是默認語言，而不是最後加入的 en。
			T(Set.Cur.Get(Key) == "登錄");
			return NIL;
		});

		R("切換後當前語言跟着變", async Arg => {
			var Key = I18nKey.Mk(null, ["Login"]);
			var Set = new I18nSet()
				.AddLang("zh-CN", [new I18nEntry(Key, "登錄")])
				.AddLang("en", [new I18nEntry(Key, "Logged in")]);
			var EnDict = Set.Use("en");
			T(EnDict.Get(Key) == "Logged in");
			T(Set.Cur.Get(Key) == "Logged in");
			T(ReferenceEquals(EnDict, Set.Cur), "Use 回傳的字典應就是當前的字典");
			Set.Use("zh-CN");
			T(Set.Cur.Get(Key) == "登錄");
			return NIL;
		});

		R("切換到沒有的語言：拋", async Arg => {
			var Set = new I18nSet().AddLang("zh-CN", new Dictionary<str, str>());
			T(Chk.Throws<ArgumentException>(() => Set.Use("ja")));
			return NIL;
		});

		R("語言清單按加入次序", async Arg => {
			var Set = new I18nSet()
				.AddLang("zh-CN", new Dictionary<str, str>())
				.AddLang("en", new Dictionary<str, str>())
				.AddLang("ja", new Dictionary<str, str>());
			var Codes = str.Join(",", Set.LangCodes);
			T(Codes == "zh-CN,en,ja", $"期望 zh-CN,en,ja，實際 {Codes}");
			return NIL;
		});
	}
}
