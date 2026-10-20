namespace Tsinswreng.CsI18n;

using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;

[Doc($$"""
鍵找不到時的兜底。

*第二個參數為何是 {{nameof(IKeyNode)}} 而不是 {{nameof(II18nKey)}}*：
查文案的鍵不只界面文案鍵一種。錯誤鍵（`Tsinswreng.CsErr.IErrNode`）本身也是 {{nameof(IKeyNode)}}，
業務代碼常常是「拿到一個異常，直接 `I18n.Get(Err.Type, Args)` 翻出給用戶看的句子」。
若這裏（以及 `Get`）收的是 {{nameof(II18nKey)}}，錯誤鍵就必須先被包成一個文案鍵才能查——
那層包裝機在每個調用點都要寫一遍，而且會讓「同一個鍵既是異常種類又是文案鍵」這件事分裂成兩個物件。
收 {{nameof(IKeyNode)}} 之後，錯誤鍵與界面文案鍵共用同一條查詢路徑，本庫也不必依賴 CsErr。

*代價（要知情）*：合約變寬，兜底處理器拿到的是「任意鍵」，
型別上不再保證它是哪一種鍵。需要區分時自行判別：
界面文案鍵可用 `Key is {{nameof(II18nKey)}}`，錯誤鍵可用 `Key is Tsinswreng.CsErr.IErrNode`。

*預設行為*：回鍵的全路徑，讓漏翻的鍵在界面上直接看得出來。
""")]
public delegate str OnKeyNotFound(II18n Self, IKeyNode Key, params obj?[] Args);

[Doc($$"""
國際化／本地化門面。

*鍵的型別故意放寬到 {{nameof(IKeyNode)}}*：界面文案鍵（{{nameof(II18nKey)}}）、
錯誤鍵（`Tsinswreng.CsErr.IErrNode` 也是 {{nameof(IKeyNode)}}）、配置鍵都能直接查，
本庫因此不必依賴 CsErr 或 CsCfg。理由與代價見 {{nameof(OnKeyNotFound)}} 的註釋。
""")]
public interface II18n {
	[Doc($$"""
	取文案並填參數。
	#Prm[{{nameof(Key)}}][任意鍵節點，其全路徑即字典鍵。]
	#Prm[{{nameof(Args)}}][位置參數。]
	""")]
	public str Get(IKeyNode Key, params obj?[] Args);

	[Doc("索引器形式：`I18n[KeysI18n.Login]`。")]
	public str this[IKeyNode Key] { get; }

	[Doc($$"""
	鍵找不到時的行為；為 null 時由實現自行決定（{{nameof(I18nDict)}} 的預設是回鍵路徑）。

	收 {{nameof(IKeyNode)}} 的理由見 {{nameof(OnKeyNotFound)}}。
	""")]
	public OnKeyNotFound? OnKeyNotFound { get; set; }
}
