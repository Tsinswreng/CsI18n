using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsI18n.Test.Utils;

namespace Tsinswreng.CsI18n.Test.Domains.Set;

// 本文件只測「語言不在時按需載入」這條縫（LangLoader／HasLang）。
// 存在的理由：每種語言的譯文若全寫進程式，沒用到的語言也佔組件體積與內存。
public partial class TestI18nSet {
	[Doc($$"""
	#Sum[掛上 {{nameof(I18nSet.LangLoader)}} 的用例。]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterLangLoader(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestI18nSet),
			[typeof(I18nSet)],
			[nameof(I18nSet.LangLoader)],
			"LangLoader:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("切到未載入的語言時才去拿", async Arg => {
			var Key = I18nKey.Mk(null, ["Login"]);
			var Loaded = new List<str>();
			var Set = new I18nSet { DfltLang = "zh-CN" }
				.AddLang("zh-CN", new Dictionary<str, str> { ["Login"] = "登錄" });
			Set.LangLoader = Lang => {
				Loaded.Add(Lang);
				return new Dictionary<str, str> {
					[Key.BuildFullPath(I18nDict.KeySep)] = "Logged in",
				};
			};
			// 還沒切過去，不該去拿。
			T(Loaded.Count == 0, $"期望尚未載入，實際 {Loaded.Count}");
			T(!Set.HasLang("en"));
			var En = Set.Use("en");
			T(En.Get(Key) == "Logged in", $"實際 {En.Get(Key)}");
			T(Loaded.Count == 1 && Loaded[0] == "en", "應該只為 en 載入一次");
			// 第二次切換用的是已載入的字典，不該再問載入器。
			Set.Use("en");
			T(Loaded.Count == 1, $"期望仍是一次，實際 {Loaded.Count}");
			return NIL;
		});

		R("默認語言也可以在第一次取用時才載入", async Arg => {
			// Cur 會自動切到 DfltLang，故這裡同樣走載入器；啟動時就只付一個語言的代價。
			var Key = I18nKey.Mk(null, ["Login"]);
			var Set = new I18nSet { DfltLang = "en" };
			Set.LangLoader = Lang => new Dictionary<str, str> {
				[Key.BuildFullPath(I18nDict.KeySep)] = "Logged in",
			};
			T(!Set.HasLang("en"));
			var Text = Set.Cur.Get(Key);
			T(Text == "Logged in", $"實際 {Text}");
			T(Set.HasLang("en"));
			return NIL;
		});

		R("沒有載入器又切到沒有的語言：拋", async Arg => {
			var Set = new I18nSet().AddLang("zh-CN", new Dictionary<str, str>());
			T(Chk.Throws<ArgumentException>(() => Set.Use("en")));
			return NIL;
		});
	}
}
