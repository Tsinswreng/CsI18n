namespace Tsinswreng.CsI18n;

using Tsinswreng.CsCore;

[Doc($$"""
多語言集合：語言代碼 → 字典，並記住「當前語言」。

*為何在庫裡*：切語言、列語言、比對覆蓋率這三件事與應用無關，
每個用 i18n 的專案都要寫一遍；放這裡之後應用只剩「給資料」。
""")]
public class I18nSet {
	[Doc("語言代碼 → 字典。")]
	public IDictionary<str, I18nDict> Dicts { get; set; } = new Dictionary<str, I18nDict>();

	[Doc("默認語言；第一個加入的語言會自動成為默認。")]
	public str DfltLang { get; set; } = "";

	[Doc("當前語言；第一次取用時切到默認語言。")]
	public II18n Cur {
		get {
			if (_Cur is null) {
				if (string.IsNullOrWhiteSpace(DfltLang)) {
					throw new InvalidOperationException(
						$"No language has been added. Add a language, or set {nameof(DfltLang)} and {nameof(LangLoader)}."
					);
				}
				Use(DfltLang);
			}
			return _Cur!;
		}
	}
	II18n? _Cur;

	[Doc("已加入的語言代碼。")]
	public IList<str> LangCodes {
		get {
			return Dicts.Keys.ToList();
		}
	}

	[Doc($$"""
	語言還沒加入時，去哪裏拿它的「鍵 → 模板」表。

	*為何需要這條縫*：把每種語言的譯文都寫進程式，沒用到的語言也會佔組件體積與內存
	（見 `Tsinswreng.CsI18n` 的 README「語言包」章）。常見做法是每種語言一個 JSON，
	只在真的切到它時才讀。本集合因此允許一個載入器：{{nameof(Use)}} 發現語言不在就調它。

	為 null 時行為照舊：{{nameof(Use)}} 直接拋。
	""")]
	public Func<str, IDictionary<str, str>>? LangLoader { get; set; }

	[Doc($$"""
	這個語言是否已加入（即已載入）。
	#Prm[{{nameof(Lang)}}][語言代碼。]
	""")]
	public bool HasLang(str Lang) {
		return Dicts.ContainsKey(Lang);
	}

	[Doc($$"""
	加入一個語言。
	#Prm[{{nameof(Lang)}}][語言代碼，例如 `zh-CN`、`en`。]
	#Prm[{{nameof(Map)}}][鍵 → 模板；C\# 宣告與 JSON 語言包都走這一個入口。]
	#Rtn[自身，便於鏈式加入。]
	""")]
	public I18nSet AddLang(str Lang, IDictionary<str, str> Map) {
		Dicts[Lang] = I18nDict.FromMap(Map);
		if (string.IsNullOrWhiteSpace(DfltLang)) {
			DfltLang = Lang;
		}
		return this;
	}

	[Doc($$"""
	加入一個 {{nameof(ILang)}}。

	*為何要有這一條*：應用宣告譯文時用的是鍵物件（改名會編譯錯），
	從 JSON 讀回來時鍵是字符串。{{nameof(ILang)}} 讓兩種來源共用同一個型別，
	鍵的規範化交給 {{nameof(I18nDict.FromLang)}}，本集合不必知道鍵是哪一種。
	#Prm[{{nameof(Lang)}}][語言；{{nameof(ILang.Code)}} 即字典代碼。]
	#Rtn[自身，便於鏈式加入。]
	""")]
	public I18nSet AddLang(ILang Lang) {
		Dicts[Lang.Code] = I18nDict.FromLang(Lang);
		if (string.IsNullOrWhiteSpace(DfltLang)) {
			DfltLang = Lang.Code;
		}
		return this;
	}
	[Doc($$"""
	切換當前語言。
	#Prm[{{nameof(Lang)}}][語言代碼。]
	#Rtn[該語言的字典。]
	#Throw[{{nameof(ArgumentException)}}][沒有這個語言、且沒有 {{nameof(LangLoader)}} 可以載入它時。]
	""")]
	public II18n Use(str Lang) {
		// step 1：語言不在就先問載入器要（每種語言一個語言包時，這裡才真的讀檔）。
		if (!Dicts.ContainsKey(Lang)) {
			if (LangLoader is null) {
				throw new ArgumentException(
					$"Language is not available: {Lang}. Add it, or set {nameof(LangLoader)} so it can be loaded on demand.",
					nameof(Lang)
				);
			}
			AddLang(Lang, LangLoader(Lang));
		}
		// step 2：切過去。
		_Cur = Dicts[Lang];
		return _Cur;
	}

	[Doc($$"""
	覆蓋率自檢：語言之間互相比對，找出缺譯與空譯文。

	*不收任何鍵清單*：必檢鍵就是「所有語言已經提供的鍵」的聯集。
	語言包本身就是事實源，故不需要、也不允許呼叫方另外維護一份鍵清單——
	那種清單要人跟著改，漏改不會編譯錯，還會讓自檢靜默漏掉那條，
	表面上有檢查、實際上檢查不到。任何手寫的 `All` 清單都因此退場。

	*檢查什麼*：某鍵在 A 語言有、B 語言沒有（缺譯）；某語言的譯文是空白（等於沒翻）。
	*不檢查什麼*：某鍵在所有語言都沒有。這種「代碼宣告了鍵、卻沒有任何語言翻它」
	看不清語言包，聯集裏根本沒有它。要抓這種洞得拿代碼側的鍵集合來對照，
	那是構建期工具的事（反射鍵類、或源生成），不是本集合能自己知道的事實。

	*一次報完，不在第一處就中斷*：所有問題收進清單一次交回，
	呼叫方看完整份再決定怎麼處理，而不是修一條、跑一次、再撞下一條。
	#Rtn[問題列表；空表示沒問題。]
	""")]
	public IList<str> Verify() {
		var R = new List<str>();
		if (Dicts.Count == 0) {
			// 沒有任何語言時回空清單會被讀成「檢查通過」，那是比報錯更壞的結果。
			R.Add("No language has been added; nothing was verified.");
			return R;
		}

		// step 1：先取聯集。所有語言提供的鍵合起來，就是「應該有哪些」的定義。
		var AllKeys = new HashSet<str>();
		foreach (var Pair in Dicts) {
			foreach (var Key in Pair.Value.Map.Keys) {
				AllKeys.Add(Key);
			}
		}

		// step 2：逐語言比對。缺譯按鍵報，空譯文按語言自己的條目報。
		foreach (var Pair in Dicts) {
			foreach (var Key in AllKeys) {
				if (!Pair.Value.Map.ContainsKey(Key)) {
					R.Add($"[{Pair.Key}] missing translation: {Key}");
				}
			}
			foreach (var Item in Pair.Value.Map) {
				if (string.IsNullOrWhiteSpace(Item.Value)) {
					R.Add($"[{Pair.Key}] empty translation: {Item.Key}");
				}
			}
		}
		return R;
	}
}
