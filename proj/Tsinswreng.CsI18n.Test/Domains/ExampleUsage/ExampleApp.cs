using Tsinswreng.CsCore;
using Tsinswreng.CsErr;
using Tsinswreng.CsKeyNode;
using K = Tsinswreng.CsErr.IErrNode;
using static Tsinswreng.CsErr.ErrNode;

namespace Tsinswreng.CsI18n.Test.Domains.ExampleUsage;

// 綜合示例的「示例應用」本體，被 TestExampleUsage 當作被測對象。
// 一個應用要用到的四樣東西（鍵／譯文／門面）按推薦結構擺好：
//   KeysI18n（界面文案、平鋪）、KeysErr（錯誤、按領域分組）、MsgZhCn／MsgEn（譯文）、AppI18n（門面）。
// 為甚麼鍵與譯文都寫在 C# 裏：鍵與譯文互相引用，改一個鍵名會讓漏改的譯文編譯不過；
// 模板又是原始字符串，多行、不必轉義。JSON 那條路留給譯文由外部工具產生的專案（見 I18nJson）。

[Doc($$"""
#Sum[界面文案鍵：*平鋪一層*，不按畫面分組。]
#Descr[
同一個鍵到哪裏都該是同一句譯文；分組會誘使人為「登錄頁的取消」與「刪除確認的取消」各立一鍵，
最後同一句話翻出多份、改一處漏三處。真需要不同措辭時，正解是加參數或另立語義不同的鍵。

*命名*：識別符即鍵字符串；成員名結尾的 `__` 表示一個位置參數（`Welcome__` 用掉 `{0}`）。
]
""")]
public static class KeysI18n {
	[Doc("造一個平鋪鍵。父節點傳 null 表示它是頂層鍵。")]
	static I18nKey Mk1(str Name) {
		return I18nKey.Mk(null, [Name]);
	}

	[Doc("應用標題。")]
	public static I18nKey AppTitle = Mk1(nameof(AppTitle));

	[Doc("「登錄」。")]
	public static I18nKey Login = Mk1(nameof(Login));

	[Doc("「退出登錄」。")]
	public static I18nKey Logout = Mk1(nameof(Logout));

	[Doc("「歡迎，{0}」。結尾 `__` 對應一個位置參數。")]
	public static I18nKey Welcome__ = Mk1(nameof(Welcome__));

	[Doc($$"""
	#Sum[全部界面文案鍵。]
	#Descr[
	NativeAOT 下不能反射列舉靜態欄位，而覆蓋率自檢需要知道全集，故手寫這份清單。
	新增鍵時同步加進來：漏了不會編譯錯，但 {{nameof(I18nSet.Verify)}} 就檢查不到它。
	]
	""")]
	public static IList<II18nKey> All = [
		AppTitle, Login, Logout, Welcome__,
	];
}

[Doc($$"""
#Sum[錯誤鍵：`Tsinswreng.CsErr` 的鍵樹，按領域分組，根是 `Error`。]
#Descr[
*錯誤鍵就用 CsErr 造*：`{{nameof(ErrNode)}}.{{nameof(ErrNode.Mk)}}(父, [本段路徑])`，型別是 `IErrNode`。
它本身就是 {{nameof(IKeyNode)}}（`IErrNode : IKeyNode`），故路徑即字典鍵——
`NotLogin` 的字典鍵是 `Error/User/NotLogin`，錯誤鍵**不必另立一份文案鍵**，譯文直接掛在同一個鍵上。
而且它可以直接拋：`throw {{nameof(KeysErr)}}.{{nameof(KeysErr.User)}}.{{nameof(KeysErr.User.PasswordNotMatch)}}.ToErr()`。

*根為甚麼另立 {{nameof(ErrRoot)}} 這個類型*：若根是外層類的成員，分組類初始化時讀它就會觸發外層類初始化，
而外層類初始化又要讀 {{nameof(KeysErr.All)}}（內含分組成員）——此時分組類正在初始化中，CLR 不重入，
成員仍是 null，{{nameof(KeysErr.All)}} 便永久帶著 null。
]
""")]
public static class ErrRoot {
	[Doc("錯誤鍵樹的根，路徑 `Error`。")]
	public static K Node = Mk(null, ["Error"]);
}

public static class KeysErr {
	[Doc("通用錯誤。")]
	public static class Common {
		[Doc("本組的根。")]
		public static K _R = Mk(ErrRoot.Node, [nameof(Common)]);

		[Doc("未預期的失敗。給用戶只說「請稍後重試」，詳細原因看服務端日誌。")]
		public static K UnknownErr = Mk(_R, [nameof(UnknownErr)]);
	}

	[Doc("用戶與認證相關。")]
	public static class User {
		[Doc("本組的根。")]
		public static K _R = Mk(ErrRoot.Node, [nameof(User)]);

		[Doc("請求未攜帶令牌，或令牌過期。")]
		public static K NotLogin = Mk(_R, [nameof(NotLogin)]);

		[Doc("身份或密碼不正確。刻意不區分「用戶不存在」與「密碼錯誤」：分開會讓攻擊者能枚舉賬號。")]
		public static K PasswordNotMatch = Mk(_R, [nameof(PasswordNotMatch)]);
	}

	[Doc("全部錯誤鍵；同樣是給覆蓋率自檢用的全集。")]
	public static IList<K> All = [
		Common.UnknownErr,
		User.NotLogin, User.PasswordNotMatch,
	];
}

[Doc("簡體中文譯文。每條都是 `new(鍵物件, 模板)`，故鍵改名時這裏編譯不過。")]
public static class MsgZhCn {
	[Doc("本語言的全部條目。")]
	public static IList<I18nEntry> Entries = [
		new(KeysI18n.AppTitle, "示例應用"),
		new(KeysI18n.Login, "登錄"),
		new(KeysI18n.Logout, "退出登錄"),
		new(KeysI18n.Welcome__, "歡迎，{0}"),
		new(KeysErr.Common.UnknownErr, "出了點問題，請稍後重試"),
		new(KeysErr.User.NotLogin, "尚未登錄"),
		new(KeysErr.User.PasswordNotMatch, "登錄名或密碼不正確"),
	];
}

[Doc("英文譯文。條目與中文一一對應——缺一條 {{nameof(I18nSet.Verify)}} 就會報出來。")]
public static class MsgEn {
	[Doc("本語言的全部條目。")]
	public static IList<I18nEntry> Entries = [
		new(KeysI18n.AppTitle, "Example App"),
		new(KeysI18n.Login, "Log in"),
		new(KeysI18n.Logout, "Log out"),
		new(KeysI18n.Welcome__, "Welcome, {0}"),
		new(KeysErr.Common.UnknownErr, "Something went wrong. Please try again later."),
		new(KeysErr.User.NotLogin, "Not logged in"),
		new(KeysErr.User.PasswordNotMatch, "Incorrect name or password"),
	];
}

[Doc($$"""
#Sum[應用的 i18n 門面：只做「哪幾種語言、當前是哪個」。]
#Descr[
字典、切換、覆蓋率自檢都在 {{nameof(I18nSet)}} 裏；本類只把語言清單釘死一處，
讓業務代碼不必知道有幾個語言。

*接進依賴注入*（真實項目在 `DiCore` 那類裝配處寫）：
```cs
Svc.AddSingleton<{{nameof(II18n)}}>({{nameof(AppI18n)}}.{{nameof(Inst)}});
```
業務代碼就只依賴 {{nameof(II18n)}}，連本類都不必認識。
]
""")]
public static class AppI18n {
	[Doc("默認語言；也是啟動時顯示的語言。")]
	public const str DfltLang = "zh-CN";

	[Doc("全部語言。")]
	public static I18nSet Set = new I18nSet { DfltLang = DfltLang }
		.AddLang("zh-CN", MsgZhCn.Entries)
		.AddLang("en", MsgEn.Entries);

	[Doc("當前語言的門面；業務代碼取文案一律走它。")]
	public static II18n Inst {
		get {
			return Set.Cur;
		}
	}

	[Doc($$"""
	#Sum[切換當前語言。]
	#Prm[{{nameof(Lang)}}][語言代碼，例如 `en`。]
	#Rtn[該語言的門面。]
	""")]
	public static II18n Use(str Lang) {
		return Set.Use(Lang);
	}
}

