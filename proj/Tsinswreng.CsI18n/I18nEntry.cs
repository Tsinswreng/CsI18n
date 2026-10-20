namespace Tsinswreng.CsI18n;

using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;

[Doc($$"""
一條譯文：鍵 ＋ 模板。

*鍵收 {nameof(IKeyNode)}*：條目由應用宣告時直接引用它自己的鍵物件（文案鍵或錯誤鍵），
鍵改名、刪除時**編譯就會錯**，不必另建一套只存在於翻譯檔的字符串鍵。
""")]
public class I18nEntry {
	[Doc("鍵物件本體；保留起來是為了必要時回溯（例如錯誤鍵的標籤）。")]
	public IKeyNode Key { get; set; } = null!;

	[Doc("鍵的規範化字符串，即字典鍵（見 {nameof(BuildKeyStr)}）。")]
	public str KeyStr { get; set; } = "";

	[Doc("模板；`{0}`、`{1}` 為位置參數。")]
	public str Text { get; set; } = "";

	[Doc($$"""
	用鍵物件造一條譯文。
	#Prm[{{nameof(Key)}}][鍵物件。]
	#Prm[{{nameof(Text)}}][模板。]
	""")]
	public I18nEntry(IKeyNode Key, str Text) {
		this.Key = Key;
		this.KeyStr = BuildKeyStr(Key);
		this.Text = Text;
	}

	[Doc($$"""
	把鍵規範化成字典鍵：父鏈路徑段用 `/` 連接。

	固定用 `/`（而不是各節點自己的 PathSep），因為同一個字典要同時容納
	界面文案鍵（平鋪）與錯誤鍵（樹狀），分隔符必須統一才不會出現「同一條目兩種寫法」。
	#Prm[{{nameof(Key)}}][任意鍵節點。]
	#Rtn[字典鍵。]
	""")]
	public static str BuildKeyStr(IKeyNode Key) {
		return Key.BuildFullPath("/");
	}
}
