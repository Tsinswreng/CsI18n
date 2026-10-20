namespace Tsinswreng.CsI18n;

using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;

[Doc($$"""
以「字典鍵 → 模板」為後端的 {nameof(II18n)} 實現。

*不讀檔、不依賴任何配置庫*：資料就是應用給的一串 {nameof(I18nEntry)}（或用 {nameof(FromMap)} 灌入的字典），
故格式在編譯期就固定、也沒有「源與產物漂移」的問題。
""")]
public class I18nDict : II18n {
	[Doc("字典鍵 → 模板。")]
	public IDictionary<str, str> Map { get; set; } = new Dictionary<str, str>();

	[Doc($$"""
	模板格式化引擎；用 {{nameof(IcuFormatter)}}（ICU MessageFormat，支援位置參數與複數）。
	要別的語法就自己實現 {{nameof(IFormatter)}} 再換掉這個值。
	""")]
	public IFormatter Formatter { get; set; } = IcuFormatter.Inst;

	[Doc("鍵找不到時的行為；預設回鍵的全路徑。")]
	public OnKeyNotFound? OnKeyNotFound { get; set; } = (Self, Key, Args) => {
		return I18nEntry.BuildKeyStr(Key);
	};

	[Doc($$"""
	按鍵取文案並填參數。
	#Prm[{{nameof(Key)}}][任意鍵節點。]
	#Prm[{{nameof(Args)}}][位置參數。]
	#Rtn[填好的文案；鍵不存在時回鍵路徑；無參數時直接回模板。]
	""")]
	public str Get(IKeyNode Key, params obj?[] Args) {
		var K = I18nEntry.BuildKeyStr(Key);
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
	由譯文條目建字典。
	#Prm[{{nameof(Entries)}}][某個語言的全部條目。]
	#Rtn[可用的字典。]
	#Throw[{{nameof(InvalidOperationException)}}][同一鍵重複出現時（那一定是複製貼上沒刪）。]
	""")]
	public static I18nDict Mk(IEnumerable<I18nEntry> Entries) {
		var R = new I18nDict();
		foreach (var Entry in Entries) {
			R.Add(Entry.KeyStr, Entry.Text);
		}
		return R;
	}

	[Doc($$"""
	由「鍵 → 模板」字典直接建（給 JSON 語言包這類外部來源用）。
	#Prm[{{nameof(Map)}}][鍵 → 模板。]
	#Rtn[可用的字典。]
	""")]
	public static I18nDict FromMap(IDictionary<str, str> Map) {
		var R = new I18nDict();
		foreach (var Pair in Map) {
			R.Add(Pair.Key, Pair.Value);
		}
		return R;
	}

	[Doc($$"""
	加一條譯文；同鍵重複即拋。
	#Prm[{{nameof(KeyStr)}}][字典鍵。]
	#Prm[{{nameof(Text)}}][模板。]
	""")]
	void Add(str KeyStr, str Text) {
		if (Map.ContainsKey(KeyStr)) {
			throw new InvalidOperationException($"譯文鍵重複：{KeyStr}");
		}
		Map[KeyStr] = Text;
	}
}

