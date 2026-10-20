namespace Tsinswreng.CsI18n;

using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;

[Doc($$"""
以「字典鍵 → 模板」為後端的 {nameof(II18n)} 實現。

*不讀檔、不依賴任何配置庫*：資料就是應用給的一張「鍵 → 模板」表（見 {nameof(FromMap)}），
故格式在編譯期就固定、也沒有「源與產物漂移」的問題。

*本庫不提供「譯文條目」型別*：譯文該怎麼宣告是應用的事——應用想用 `record`、
想用字典、想從 JSON 讀都行，只要最後交出「鍵 → 模板」。本庫只認最終形態，
不替應用規定宣告期的資料形狀。
""")]
public class I18nDict : II18n {
	[Doc($$"""
	字典鍵的分隔符，固定 `/`。

	*為何固定在庫裏、不讓各處自己拼*：同一個字典要同時容納平鋪的界面文案鍵
	（`Login`）與樹狀的錯誤鍵（`Error/User/NotLogin`），分隔符必須統一，
	否則同一條譯文會有兩種寫法、C\# 寫的譯文與 JSON 語言包也對不上。
	誰拼字典鍵都必須用這個常量。
	""")]
	public const str KeySep = "/";

	[Doc("字典鍵 → 模板。")]
	public IDictionary<str, str> Map { get; set; } = new Dictionary<str, str>();

	[Doc($$"""
	模板格式化引擎；用 {{nameof(IcuFormatter)}}（ICU MessageFormat，支援位置參數與複數）。
	要別的語法就自己實現 {{nameof(IFormatter)}} 再換掉這個值。
	""")]
	public IFormatter Formatter { get; set; } = IcuFormatter.Inst;

	[Doc("鍵找不到時的行為；預設回鍵的全路徑。")]
	public OnKeyNotFound? OnKeyNotFound { get; set; } = (Self, Key, Args) => {
		return Key.BuildFullPath(KeySep);
	};

	[Doc($$"""
	按鍵取文案並填參數。
	#Prm[{{nameof(Key)}}][任意鍵節點。]
	#Prm[{{nameof(Args)}}][位置參數。]
	#Rtn[填好的文案；鍵不存在時回鍵路徑；無參數時直接回模板。]
	""")]
	public str Get(IKeyNode Key, params obj?[] Args) {
		var K = Key.BuildFullPath(KeySep);
		if (!Map.TryGetValue(K, out var Template)) {
			return OnKeyNotFound?.Invoke(this, Key, Args) ?? K;
		}
		// 沒有參數就沒什麼好填的：直接回原文，也讓自訂 Formatter 不必處理空參數。
		if (Args.Length == 0) {
			return Template;
		}
		return Formatter.Format(Template, Args);
	}

	[Doc("索引器形式。")]
	public str this[IKeyNode Key] {
		get {
			return Get(Key);
		}
	}

	[Doc($$"""
	由「鍵 → 模板」字典建字典（給 C\# 宣告、JSON 語言包等任何來源用）。

	*為何不另立「條目」型別*：條目的形狀是應用自己的事；本庫收到的終究是「鍵 → 模板」。
	應用想保留「鍵物件」帶來的編譯期檢查，就在自己那層用 record 宣告、再攤成這張表。
	#Prm[{{nameof(Map)}}][鍵 → 模板；鍵必須已用 {nameof(KeySep)} 規範化。]
	#Rtn[可用的字典。]
	""")]
	public static I18nDict FromMap(IDictionary<str, str> Map) {
		// 鍵的唯一性由 IDictionary 本身保證，這裏不必再檢查一次。
		return new I18nDict { Map = new Dictionary<str, str>(Map) };
	}
}
