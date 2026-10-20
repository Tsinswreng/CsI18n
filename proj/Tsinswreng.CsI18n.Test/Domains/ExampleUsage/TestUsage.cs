using Tsinswreng.CsCore;
using Tsinswreng.CsErr;
using Tsinswreng.CsKeyNode;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.ExampleUsage;

// 本文件就是那個「綜合示例」：一條用例、從頭讀到尾，展示本庫的典型用法。
// 示例應用的擺法在 ExampleApp.cs（鍵／譯文／門面）。
public partial class TestExampleUsage {
	[Doc($$"""
	#Sum[掛上綜合示例。]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterUsage(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestExampleUsage),
			[typeof(II18n), typeof(I18nSet)],
			[nameof(II18n.Get), nameof(I18nSet.Use), nameof(I18nSet.Verify)],
			"用法:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("綜合示例：從取文案到覆蓋率自檢", async Arg => {
			// ── 步驟 1：按鍵取界面文案 ───────────────────────────────
			// 業務代碼只依賴 II18n；鍵是強型別物件，不是字符串。
			// 索引器等價於「無參數的 Get」。
			AppI18n.Use(AppI18n.DfltLang);
			var Title = AppI18n.Inst[KeysI18n.AppTitle];
			var Login = AppI18n.Inst[KeysI18n.Login];
			T(Title == "示例應用", $"步驟1 期望 示例應用，實際 {Title}");
			T(Login == "登錄", $"步驟1 期望 登錄，實際 {Login}");

			// ── 步驟 2：切換語言 ─────────────────────────────────────
			// 切換隻改「當前語言」；已渲染的界面要自己重繪（本庫不管界面）。
			var En = AppI18n.Use("en");
			T(En[KeysI18n.Login] == "Log in", $"步驟2 實際 {En[KeysI18n.Login]}");
			// 門面跟着換了，故之後取文案都是英文。
			T(AppI18n.Inst[KeysI18n.AppTitle] == "Example App", $"步驟2 實際 {AppI18n.Inst[KeysI18n.AppTitle]}");

			// ── 步驟 3：帶參數的文案 ─────────────────────────────────
			// 模板用位置參數 {0}、{1}；鍵名結尾一個 __ 對應一個參數。
			// 要 ICU（複數、性別、選擇）就換掉 I18nDict.Formatter，呼叫端不用動。
			var EnWelcome = AppI18n.Inst.Get(KeysI18n.Welcome__, "小明");
			T(EnWelcome == "Welcome, 小明", $"步驟3 實際 {EnWelcome}");
			AppI18n.Use(AppI18n.DfltLang);
			var ZhWelcome = AppI18n.Inst.Get(KeysI18n.Welcome__, "小明");
			T(ZhWelcome == "歡迎，小明", $"步驟3 實際 {ZhWelcome}");

			// ── 步驟 4：錯誤鍵與界面文案共用同一套譯文 ───────────────
			// 錯誤鍵由 CsErr 造（鍵樹上的一段路徑，例如 Error/User/...），不必再立一份「錯誤文案鍵」。
			// 拋的時候帶參數，前端拿 Err.Type 當鍵、Err.Args 當參數，直接翻出給用戶看的句子。
			var Err = KeysErr.User.PasswordNotMatch.ToErr();
			var ErrKeyStr = Err.Type.BuildFullPath(I18nDict.KeySep);
			T(ErrKeyStr == "Error/User/PasswordNotMatch", $"步驟4 期望 Error/User/PasswordNotMatch，實際 {ErrKeyStr}");
			var ZhErr = AppI18n.Inst.Get(Err.Type, Err.Args ?? []);
			T(ZhErr == "登錄名或密碼不正確", $"步驟4 實際 {ZhErr}");
			AppI18n.Use("en");
			var EnErr = AppI18n.Inst.Get(Err.Type, Err.Args ?? []);
			T(EnErr == "Incorrect name or password", $"步驟4 實際 {EnErr}");
			AppI18n.Use(AppI18n.DfltLang);

			// ── 步驟 5：漏翻的鍵直接顯示鍵名 ─────────────────────────
			// 預設兜底是「回鍵的全路徑」，故漏翻在界面上看得見、不會靜默變空白。
			// 想改成寫日誌或顯示別的提示，就換 II18n.OnKeyNotFound。
			var Missing = I18nKey.Mk(null, ["NotTranslatedYet"]);
			var MissingText = AppI18n.Inst[Missing];
			T(MissingText == "NotTranslatedYet", $"步驟5 期望鍵名，實際 {MissingText}");

			// ── 步驟 6：啟動時跑覆蓋率自檢 ───────────────────────────
			// 這是本庫最有價值的一條：界面文案鍵一組、錯誤鍵一組，
			// 看看每個語言是否都翻了、有沒有翻成空白。真實項目在 DI 裝配處跑一次並記日誌。
			var Problems = AppI18n.Set.Verify(KeysI18n.All, KeysErr.All);
			T(Problems.Count == 0, $"步驟6 示例應全部翻齊，實際 {Problems.Count} 個問題：{str.Join(" | ", Problems)}");

			// ── 步驟 7：外部語言包（JSON）也能併進來 ─────────────────
			// 譯文由外部工具產生時走這條：I18nJson 攤平成「a/b/c → 模板」，再 AddLang 灌進集合。
			// 攤平規則與 I18nDict.KeySep 一致，故 C# 那套鍵查得到 JSON 那套譯文。
			var JaJson = $$"""
				{
					"{{KeysI18n.Login.BuildFullPath(I18nDict.KeySep)}}": "ログイン",
					"{{KeysI18n.Welcome__.BuildFullPath(I18nDict.KeySep)}}": "{0}さん、ようこそ"
				}
				""";
			var JaSet = new I18nSet().AddLang("ja", I18nJson.FromJson(JaJson));
			var JaLogin = JaSet.Cur[KeysI18n.Login];
			var JaWelcome = JaSet.Cur.Get(KeysI18n.Welcome__, "小明");
			T(JaLogin == "ログイン", $"步驟7 實際 {JaLogin}");
			T(JaWelcome == "小明さん、ようこそ", $"步驟7 實際 {JaWelcome}");

			return NIL;
		});
	}
}

