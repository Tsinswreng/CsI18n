namespace Tsinswreng.CsI18n;

using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;

[Doc($$"""
界面文案鍵：以 KeyNode 的分段路徑為基礎。

*為何繼承 {nameof(KeyNode)}*：字典鍵就是「父鏈上的路徑段用分隔符連起來」，
這件事 CsKeyNode 已經做好；本庫不必再自造一套鍵模型，也不必依賴 CsCfg。
""")]
public class I18nKey : KeyNode, II18nKey {
	[Doc($$"""
	造一個文案鍵。
	#Prm[{{nameof(Parent)}}][父節點；頂層鍵傳 null。]
	#Prm[{{nameof(Path)}}][自己的路徑段（通常是單段）。]
	#Rtn[新的文案鍵。]
	""")]
	public static I18nKey Mk(
		IKeyNode? Parent
		, IReadOnlyList<str> Path
	) {
		return new I18nKey {
			RelaPathSegs = Path,
			Parent = Parent,
		};
	}
}
