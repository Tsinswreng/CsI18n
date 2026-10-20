using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Utils;

[Doc($$"""
#Sum[領域斷言：本庫的測試除了「條件成立」還要驗「該拋的時候真的拋」。]
#Descr[
{{nameof(Assert)}} 只給 `IsTrue`（框架的取捨：領域斷言由使用方自己寫），
故這裏補一個「期望拋指定型別異常」的斷言，免得每個用例都手寫 try/catch。

*只認型別，不認消息*：消息是給人看的、隨時可以改；
用例若綁死消息文本，改一句文案就會紅一片。
]
""")]
public static class Chk {
	[Doc($$"""
	#Sum[執行 {{nameof(Fn)}}，斷言它拋出 {{nameof(TErr)}}（或其子類）。]
	#TPrm[{{nameof(TErr)}}][期望的異常型別。]
	#Prm[{{nameof(Fn)}}][被執行的動作。]
	#Rtn[`true` 表示確實拋了期望的型別；沒拋、或拋了別的型別都是 `false`。]
	#Eg[
	```cs
	var T = Assert.IsTrue;
	T(Chk.Throws<InvalidOperationException>(() => I18nDict.Mk(Entries)));
	```
	]
	""")]
	public static bool Throws<TErr>(Action Fn) where TErr : Exception {
		// step 1：正常返回 ⇒ 沒拋，直接判定失敗。
		try {
			Fn();
		}
		// step 2：拋了期望型別（含子類）⇒ 通過。
		catch (TErr) {
			return true;
		}
		// step 3：拋了別的型別 ⇒ 不是期望的行為。
		catch {
			return false;
		}
		return false;
	}
}
