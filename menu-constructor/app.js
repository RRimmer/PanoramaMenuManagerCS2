const DEFAULTS = {
  name: "Сакура",
  accent: "#FFA9DE",
  accentSoft: "rgba(96, 128, 255, 0.40)",
  frame: "rgba(140, 170, 210, 0.22)",
  btnBg: "rgba(255, 255, 255, 0.05)",
  btnHover: "rgba(255, 255, 255, 0.11)",
  btnActive: "rgba(255, 255, 255, 0.18)",
  line: "rgba(217, 217, 217, 0.04)",
  label: "#8b8f94",
  bgTop: "rgba(22, 30, 42, 0.82)",
  bgBottom: "rgba(13, 19, 28, 0.86)",
  itemText: "#cdd8e4",
  itemValue: "#8ea2b4",
  icon: "#9fb0be",
  arrow: "#8b929a",
  disabledBg: "rgba(6, 10, 16, 0.62)",
  disabledBorder: "rgba(110, 125, 140, 0.30)",
  disabledText: "#aab6c4",
  switchOff: "rgba(0, 0, 0, 0.45)",
  knob: "#cfd4d9",
  knobOn: "#ffffff",
  switchOnLinked: true,
  switchOn: "#FFA9DE",
  optText: "#cfd4d9",
  optHover: "rgba(70, 70, 74, 0.9)",
  optActive: "rgba(90, 90, 96, 0.95)",
  selectBg: "rgba(34, 34, 36, 0.99)",
  selectBorder: "rgba(255, 255, 255, 0.06)",
  ntfOk: "#57e08a",
  ntfWarn: "#ffcf5c",
  ntfErr: "#ff5f6d",
  ntfBgTop: "rgba(24, 32, 45, 0.94)",
  ntfBgBottom: "rgba(15, 21, 31, 0.96)",
  titleLinked: true,
  titleColor: "#FFA9DE",
  menuWidth: 400,
  menuRadius: 4,
  btnRadius: 5,
  titleSize: 24,
  titleTracking: 3,
  itemSize: 17,
  descSize: 16,
  itemHeight: 40,
  barWidth: 4,
  anim: 0.15,
  blur: 2,
  dotOpacity: 0.03,
  leftBar: true,
  rightBar: true,
  showDots: true,
  showDesc: true,
  showPagination: true,
  uppercaseTitle: true,
  uppercaseItems: true,
};

const PRESETS = [
  { id: "sakura", name: "Сакура", swatch: "#FFA9DE", theme: {} },
  {
    id: "ice",
    name: "Лёд",
    swatch: "#7ED4FF",
    theme: {
      accent: "#7ED4FF",
      accentSoft: "rgba(126, 212, 255, 0.35)",
      frame: "rgba(126, 180, 210, 0.28)",
      bgTop: "rgba(12, 28, 44, 0.88)",
      bgBottom: "rgba(7, 16, 28, 0.92)",
      label: "#8eabbf",
      itemText: "#d7eefb",
      itemValue: "#8ec0d6",
      icon: "#a9d0e4",
      arrow: "#8eafc2",
    },
  },
  {
    id: "gold",
    name: "Золото",
    swatch: "#E6C15A",
    theme: {
      accent: "#E6C15A",
      accentSoft: "rgba(230, 193, 90, 0.34)",
      frame: "rgba(210, 180, 120, 0.28)",
      bgTop: "rgba(36, 28, 14, 0.90)",
      bgBottom: "rgba(18, 14, 8, 0.94)",
      label: "#b5a58a",
      itemText: "#f6ead0",
      itemValue: "#cbb892",
      icon: "#d4c09a",
      arrow: "#b3a184",
      ntfOk: "#d6e27a",
    },
  },
  {
    id: "crimson",
    name: "Кармин",
    swatch: "#FF5A6A",
    theme: {
      accent: "#FF5A6A",
      accentSoft: "rgba(255, 90, 106, 0.34)",
      frame: "rgba(220, 120, 130, 0.28)",
      bgTop: "rgba(36, 14, 18, 0.90)",
      bgBottom: "rgba(16, 8, 10, 0.94)",
      label: "#c4a0a4",
      itemText: "#ffd8dc",
      itemValue: "#e0a8ae",
      icon: "#e4b4b8",
      arrow: "#c4989c",
    },
  },
  {
    id: "toxic",
    name: "Токсин",
    swatch: "#3EE59A",
    theme: {
      accent: "#3EE59A",
      accentSoft: "rgba(62, 229, 154, 0.30)",
      frame: "rgba(80, 190, 140, 0.28)",
      bgTop: "rgba(10, 28, 20, 0.90)",
      bgBottom: "rgba(6, 14, 10, 0.94)",
      label: "#8fb8a4",
      itemText: "#d9ffee",
      itemValue: "#8ed4b4",
      icon: "#9ad8bc",
      arrow: "#7eae98",
    },
  },
  {
    id: "neon",
    name: "Неон",
    swatch: "#C084FC",
    theme: {
      accent: "#C084FC",
      accentSoft: "rgba(192, 132, 252, 0.34)",
      frame: "rgba(170, 130, 220, 0.30)",
      bgTop: "rgba(24, 14, 40, 0.90)",
      bgBottom: "rgba(10, 8, 20, 0.94)",
      label: "#b3a4c8",
      itemText: "#f0e6ff",
      itemValue: "#c4b0dc",
      icon: "#d0bce8",
      arrow: "#aa98c4",
    },
  },
  {
    id: "mono",
    name: "Моно",
    swatch: "#F2F2F2",
    theme: {
      accent: "#F2F2F2",
      accentSoft: "rgba(255, 255, 255, 0.16)",
      frame: "rgba(255, 255, 255, 0.14)",
      bgTop: "rgba(18, 18, 20, 0.92)",
      bgBottom: "rgba(8, 8, 10, 0.95)",
      label: "#9a9a9e",
      itemText: "#ececec",
      itemValue: "#b0b0b4",
      icon: "#c8c8cc",
      arrow: "#9a9a9e",
      btnBg: "rgba(255, 255, 255, 0.04)",
      btnHover: "rgba(255, 255, 255, 0.10)",
      btnActive: "rgba(255, 255, 255, 0.16)",
      showDots: false,
    },
  },
];

const PAGES = [
  [
    { kind: "submenu", label: "item.players" },
    { kind: "submenu", label: "item.punish" },
    { kind: "toggle", label: "item.sound", on: true },
    { kind: "select", label: "item.lang", value: "opt.ru", options: ["opt.ru", "opt.en", "opt.tr", "opt.zh"] },
    { kind: "value", label: "item.prefix", value: "[ADMIN]", raw: true },
    { kind: "empty" },
  ],
  [
    { kind: "submenu", label: "item.server" },
    { kind: "toggle", label: "item.ads", on: false },
    { kind: "value", label: "item.online", value: "12 / 32", raw: true },
    { kind: "select", label: "item.style", value: "opt.panorama", options: ["opt.panorama", "opt.buttons", "opt.chat"] },
    { kind: "submenu", label: "item.settings", disabled: true },
    { kind: "empty" },
  ],
];

const NTF = {
  success: { cls: "success", icon: "✓", title: "ntf.ok.title", text: "ntf.ok.text" },
  warning: { cls: "warning", icon: "!", title: "ntf.warn.title", text: "ntf.warn.text" },
  error: { cls: "error", icon: "×", title: "ntf.err.title", text: "ntf.err.text" },
};

const I18N = {
  ru: {
    "meta.title": "Конструктор панорамы — MenuManager",
    "brand.title": "Конструктор панорамы",
    "brand.sub": "MenuManagerCore · внешний вид меню",
    "btn.changes": "Список изменений",
    "btn.reset": "Сброс",
    "btn.import": "Импорт",
    "btn.json": "Тема JSON",
    "btn.css": "Только ui.css",
    "btn.zip": "Скачать панораму",
    "note": "Цвета, полоски, переключатели и размеры сразу видны справа. В архив попадает готовая панорама: <code>ui.css</code> и <code>menu_ui.xml</code>.",
    "help.title": "Как поставить на сервер",
    "help.1": "Распакуй <code>custom_menu.zip</code> в корень клиентского аддона, который игроки получают через MultiAddonManager. Не клади файлы россыпью в папку dedicated-сервера.",
    "help.2": "Внутри должны быть <code>panorama/layout/custom_game/menu_ui.xml</code> и <code>panorama/styles/custom_game/ui.css</code>.",
    "help.3": "В <code>MenuManagerCore.json</code> оставь <code>\"DefaultMenu\": \"PanoramaMenu\"</code>. Потом пересобери аддон и смени карту.",
    "help.4": "«Название: Вкл» рисуется слайдером. Список до 12 значений — выпадающий список, строка остаётся на месте. Обычный пункт — кнопка со стрелкой. Уведомление справа сверху: галочка, восклицательный знак или крестик.",
    "preview.titleLabel": "Заголовок превью",
    "preview.descLabel": "Описание",
    "preview.title": "Админ",
    "preview.desc": "Управление сервером",
    "preview.menu": "Меню",
    "ntf.success": "Успех",
    "ntf.warning": "Внимание",
    "ntf.error": "Ошибка",
    "ntf.show": "Уведомление",
    "watermark": "Превью меню",
    "caption": "Заголовок и описание здесь только для превью, в файл они не пишутся. Кликни переключатель, список и страницы — так выглядит тема. Крестик проигрывает закрытие.",
    "install.title": "Панорама скачана",
    "install.1": "<code>custom_menu.zip</code> положи в корень клиентского аддона MultiAddonManager. Игроки должны получить оба файла:",
    "install.2": "В <code>MenuManagerCore.json</code> нужно <code>\"DefaultMenu\": \"PanoramaMenu\"</code>. После замены пересобери аддон и смени карту.",
    "install.close": "Понятно",
    "aria.back": "Назад",
    "aria.close": "Закрыть",
    "aria.prev": "Предыдущая",
    "aria.next": "Следующая",
    "theme.name": "Название темы",
    "tip.open": "Открыть раздел",
    "empty": "Пустой слот",
    "unit.none": "нет",
    "unit.sec": "с",
    "import.error": "Не получилось прочитать файл темы. Нужен JSON из кнопки «Тема JSON».",
    "preset.sakura": "Сакура",
    "preset.ice": "Лёд",
    "preset.gold": "Золото",
    "preset.crimson": "Кармин",
    "preset.toxic": "Токсин",
    "preset.neon": "Неон",
    "preset.mono": "Моно",
    "group.colors": "Цвета меню",
    "group.buttons": "Кнопки",
    "group.switch": "Переключатель",
    "group.list": "Список",
    "group.disabled": "Пустой и выключенный пункт",
    "group.notify": "Уведомление",
    "group.size": "Размеры",
    "group.show": "Показать в меню",
    "field.accent": "Акцент",
    "field.titleLinked": "Заголовок цветом акцента",
    "field.titleColor": "Цвет заголовка",
    "field.bgTop": "Фон сверху",
    "field.bgBottom": "Фон снизу",
    "field.line": "Линии",
    "field.frame": "Рамка подсказки",
    "field.label": "Подписи",
    "field.btnBg": "Фон кнопки",
    "field.btnHover": "Наведение",
    "field.btnActive": "Нажатие",
    "field.itemText": "Текст пункта",
    "field.itemValue": "Значение справа",
    "field.icon": "Крестик и назад",
    "field.arrow": "Стрелки",
    "field.switchOnLinked": "Включённый — цветом акцента",
    "field.switchOn": "Дорожка вкл",
    "field.switchOff": "Дорожка выкл",
    "field.knob": "Ползунок",
    "field.knobOn": "Ползунок вкл",
    "field.selectBg": "Фон списка",
    "field.selectBorder": "Рамка списка",
    "field.optText": "Текст варианта",
    "field.optHover": "Наведение на вариант",
    "field.optActive": "Нажатие на вариант",
    "field.accentSoft": "Выбранный вариант",
    "field.disabledBg": "Фон",
    "field.disabledBorder": "Пунктир",
    "field.disabledText": "Текст",
    "field.ntfOk": "Успех",
    "field.ntfWarn": "Внимание",
    "field.ntfErr": "Ошибка",
    "field.ntfBgTop": "Фон сверху",
    "field.ntfBgBottom": "Фон снизу",
    "field.menuWidth": "Ширина меню",
    "field.menuRadius": "Скругление меню",
    "field.btnRadius": "Скругление кнопок",
    "field.titleSize": "Размер заголовка",
    "field.titleTracking": "Разрядка заголовка",
    "field.itemSize": "Размер текста",
    "field.descSize": "Размер описания",
    "field.itemHeight": "Высота пункта",
    "field.barWidth": "Толщина полос",
    "field.anim": "Скорость анимации",
    "field.blur": "Размытие фона",
    "field.dotOpacity": "Сила точек",
    "field.leftBar": "Полоска слева",
    "field.rightBar": "Полоска справа",
    "field.showDots": "Точки на фоне",
    "field.showDesc": "Строка описания",
    "field.showPagination": "Страницы снизу",
    "field.uppercaseTitle": "Заголовок капсом",
    "field.uppercaseItems": "Пункты капсом",
    "item.players": "Игроки",
    "item.punish": "Наказания",
    "item.sound": "Звук меню",
    "item.lang": "Язык",
    "item.prefix": "Префикс",
    "item.server": "Сервер",
    "item.ads": "Реклама",
    "item.online": "Онлайн",
    "item.style": "Стиль",
    "item.settings": "Настройки",
    "opt.ru": "Русский",
    "opt.en": "English",
    "opt.tr": "Türkçe",
    "opt.zh": "中文",
    "opt.panorama": "Панорама",
    "opt.buttons": "Кнопки",
    "opt.chat": "Чат",
    "ntf.ok.title": "Готово",
    "ntf.ok.text": "Игрок кикнут с сервера",
    "ntf.warn.title": "Внимание",
    "ntf.warn.text": "Мало свободных слотов",
    "ntf.err.title": "Ошибка",
    "ntf.err.text": "Не удалось выполнить",
  },
  en: {
    "meta.title": "Panorama constructor — MenuManager",
    "brand.title": "Panorama constructor",
    "brand.sub": "MenuManagerCore · menu look",
    "btn.changes": "Changelog",
    "btn.reset": "Reset",
    "btn.import": "Import",
    "btn.json": "Theme JSON",
    "btn.css": "ui.css only",
    "btn.zip": "Download panorama",
    "note": "Colors, bars, switches, and sizes update the preview on the right. The archive contains the finished panorama: <code>ui.css</code> and <code>menu_ui.xml</code>.",
    "help.title": "How to install",
    "help.1": "Extract <code>custom_menu.zip</code> into the root of the client addon that players receive through MultiAddonManager. Do not drop the files loose on the dedicated server.",
    "help.2": "The addon needs <code>panorama/layout/custom_game/menu_ui.xml</code> and <code>panorama/styles/custom_game/ui.css</code>.",
    "help.3": "Keep <code>\"DefaultMenu\": \"PanoramaMenu\"</code> in <code>MenuManagerCore.json</code>. Then rebuild the addon and change the map.",
    "help.4": "A row shaped like “name: on/off” is drawn as a switch. A list of up to 12 values is a dropdown, and the row stays in place. Other rows stay as buttons with an arrow. The toast at the top right uses a check, an exclamation mark or a cross.",
    "preview.titleLabel": "Preview title",
    "preview.descLabel": "Description",
    "preview.title": "Admin",
    "preview.desc": "Server control",
    "preview.menu": "Menu",
    "ntf.success": "Success",
    "ntf.warning": "Warning",
    "ntf.error": "Error",
    "ntf.show": "Notification",
    "watermark": "Menu preview",
    "caption": "The title and description here are preview only and are not written into the file. Click the switch, the list, and the pages to see the theme. The close icon plays the dismiss animation.",
    "install.title": "Panorama downloaded",
    "install.1": "Put <code>custom_menu.zip</code> at the root of the MultiAddonManager client addon. Players need both files:",
    "install.2": "<code>MenuManagerCore.json</code> needs <code>\"DefaultMenu\": \"PanoramaMenu\"</code>. After replacing the files, rebuild the addon and change the map.",
    "install.close": "Got it",
    "aria.back": "Back",
    "aria.close": "Close",
    "aria.prev": "Previous",
    "aria.next": "Next",
    "theme.name": "Theme name",
    "tip.open": "Open section",
    "empty": "Empty slot",
    "unit.none": "off",
    "unit.sec": "s",
    "import.error": "Could not read the theme file. Use the JSON from the Theme JSON button.",
    "preset.sakura": "Sakura",
    "preset.ice": "Ice",
    "preset.gold": "Gold",
    "preset.crimson": "Crimson",
    "preset.toxic": "Toxic",
    "preset.neon": "Neon",
    "preset.mono": "Mono",
    "group.colors": "Menu colors",
    "group.buttons": "Buttons",
    "group.switch": "Switch",
    "group.list": "List",
    "group.disabled": "Empty and disabled row",
    "group.notify": "Notification",
    "group.size": "Sizes",
    "group.show": "Show in the menu",
    "field.accent": "Accent",
    "field.titleLinked": "Title uses accent",
    "field.titleColor": "Title color",
    "field.bgTop": "Background top",
    "field.bgBottom": "Background bottom",
    "field.line": "Lines",
    "field.frame": "Tooltip frame",
    "field.label": "Captions",
    "field.btnBg": "Button background",
    "field.btnHover": "Hover",
    "field.btnActive": "Pressed",
    "field.itemText": "Row text",
    "field.itemValue": "Value on the right",
    "field.icon": "Close and back",
    "field.arrow": "Arrows",
    "field.switchOnLinked": "On state uses accent",
    "field.switchOn": "Track on",
    "field.switchOff": "Track off",
    "field.knob": "Knob",
    "field.knobOn": "Knob on",
    "field.selectBg": "List background",
    "field.selectBorder": "List frame",
    "field.optText": "Option text",
    "field.optHover": "Option hover",
    "field.optActive": "Option pressed",
    "field.accentSoft": "Selected option",
    "field.disabledBg": "Background",
    "field.disabledBorder": "Dashed border",
    "field.disabledText": "Text",
    "field.ntfOk": "Success",
    "field.ntfWarn": "Warning",
    "field.ntfErr": "Error",
    "field.ntfBgTop": "Background top",
    "field.ntfBgBottom": "Background bottom",
    "field.menuWidth": "Menu width",
    "field.menuRadius": "Menu corner radius",
    "field.btnRadius": "Button corner radius",
    "field.titleSize": "Title size",
    "field.titleTracking": "Title spacing",
    "field.itemSize": "Text size",
    "field.descSize": "Description size",
    "field.itemHeight": "Row height",
    "field.barWidth": "Bar thickness",
    "field.anim": "Animation speed",
    "field.blur": "Background blur",
    "field.dotOpacity": "Dot strength",
    "field.leftBar": "Left bar",
    "field.rightBar": "Right bar",
    "field.showDots": "Background dots",
    "field.showDesc": "Description line",
    "field.showPagination": "Pages at the bottom",
    "field.uppercaseTitle": "Uppercase title",
    "field.uppercaseItems": "Uppercase rows",
    "item.players": "Players",
    "item.punish": "Punishments",
    "item.sound": "Menu sound",
    "item.lang": "Language",
    "item.prefix": "Prefix",
    "item.server": "Server",
    "item.ads": "Ads",
    "item.online": "Online",
    "item.style": "Style",
    "item.settings": "Settings",
    "opt.ru": "Русский",
    "opt.en": "English",
    "opt.tr": "Türkçe",
    "opt.zh": "中文",
    "opt.panorama": "Panorama",
    "opt.buttons": "Buttons",
    "opt.chat": "Chat",
    "ntf.ok.title": "Done",
    "ntf.ok.text": "Player kicked from the server",
    "ntf.warn.title": "Warning",
    "ntf.warn.text": "Few free slots left",
    "ntf.err.title": "Error",
    "ntf.err.text": "Could not complete that",
  },
};

const COLOR_RE = /^(#[0-9a-fA-F]{6}|rgba\(\s*\d+\s*,\s*\d+\s*,\s*\d+\s*,\s*(?:0|1|0?\.\d+)\s*\))$/;

const GROUPS = [
  {
    title: "group.colors",
    fields: [
      { type: "color", key: "accent", label: "field.accent" },
      { type: "toggle", key: "titleLinked", label: "field.titleLinked" },
      { type: "color", key: "titleColor", label: "field.titleColor", off: "titleLinked" },
      { type: "color", key: "bgTop", label: "field.bgTop", alpha: true },
      { type: "color", key: "bgBottom", label: "field.bgBottom", alpha: true },
      { type: "color", key: "line", label: "field.line", alpha: true },
      { type: "color", key: "frame", label: "field.frame", alpha: true },
      { type: "color", key: "label", label: "field.label" },
    ],
  },
  {
    title: "group.buttons",
    fields: [
      { type: "color", key: "btnBg", label: "field.btnBg", alpha: true },
      { type: "color", key: "btnHover", label: "field.btnHover", alpha: true },
      { type: "color", key: "btnActive", label: "field.btnActive", alpha: true },
      { type: "color", key: "itemText", label: "field.itemText" },
      { type: "color", key: "itemValue", label: "field.itemValue" },
      { type: "color", key: "icon", label: "field.icon" },
      { type: "color", key: "arrow", label: "field.arrow" },
    ],
  },
  {
    title: "group.switch",
    fields: [
      { type: "toggle", key: "switchOnLinked", label: "field.switchOnLinked" },
      { type: "color", key: "switchOn", label: "field.switchOn", off: "switchOnLinked" },
      { type: "color", key: "switchOff", label: "field.switchOff", alpha: true },
      { type: "color", key: "knob", label: "field.knob" },
      { type: "color", key: "knobOn", label: "field.knobOn" },
    ],
  },
  {
    title: "group.list",
    fields: [
      { type: "color", key: "selectBg", label: "field.selectBg", alpha: true },
      { type: "color", key: "selectBorder", label: "field.selectBorder", alpha: true },
      { type: "color", key: "optText", label: "field.optText" },
      { type: "color", key: "optHover", label: "field.optHover", alpha: true },
      { type: "color", key: "optActive", label: "field.optActive", alpha: true },
      { type: "color", key: "accentSoft", label: "field.accentSoft", alpha: true },
    ],
  },
  {
    title: "group.disabled",
    fields: [
      { type: "color", key: "disabledBg", label: "field.disabledBg", alpha: true },
      { type: "color", key: "disabledBorder", label: "field.disabledBorder", alpha: true },
      { type: "color", key: "disabledText", label: "field.disabledText" },
    ],
  },
  {
    title: "group.notify",
    fields: [
      { type: "color", key: "ntfOk", label: "field.ntfOk" },
      { type: "color", key: "ntfWarn", label: "field.ntfWarn" },
      { type: "color", key: "ntfErr", label: "field.ntfErr" },
      { type: "color", key: "ntfBgTop", label: "field.ntfBgTop", alpha: true },
      { type: "color", key: "ntfBgBottom", label: "field.ntfBgBottom", alpha: true },
    ],
  },
  {
    title: "group.size",
    fields: [
      { type: "range", key: "menuWidth", label: "field.menuWidth", min: 320, max: 560, step: 2, unit: "px" },
      { type: "range", key: "menuRadius", label: "field.menuRadius", min: 0, max: 20, step: 1, unit: "px" },
      { type: "range", key: "btnRadius", label: "field.btnRadius", min: 0, max: 16, step: 1, unit: "px" },
      { type: "range", key: "titleSize", label: "field.titleSize", min: 16, max: 34, step: 1, unit: "px" },
      { type: "range", key: "titleTracking", label: "field.titleTracking", min: 0, max: 8, step: 1, unit: "px" },
      { type: "range", key: "itemSize", label: "field.itemSize", min: 13, max: 22, step: 1, unit: "px" },
      { type: "range", key: "descSize", label: "field.descSize", min: 12, max: 22, step: 1, unit: "px" },
      { type: "range", key: "itemHeight", label: "field.itemHeight", min: 32, max: 52, step: 1, unit: "px" },
      { type: "range", key: "barWidth", label: "field.barWidth", min: 1, max: 10, step: 1, unit: "px" },
      { type: "range", key: "anim", label: "field.anim", min: 0, max: 0.4, step: 0.01, unit: "sec" },
      { type: "range", key: "blur", label: "field.blur", min: 0, max: 8, step: 1, unit: "" },
      { type: "range", key: "dotOpacity", label: "field.dotOpacity", min: 0, max: 0.2, step: 0.01, unit: "" },
    ],
  },
  {
    title: "group.show",
    fields: [
      { type: "toggle", key: "leftBar", label: "field.leftBar" },
      { type: "toggle", key: "rightBar", label: "field.rightBar" },
      { type: "toggle", key: "showDots", label: "field.showDots" },
      { type: "toggle", key: "showDesc", label: "field.showDesc" },
      { type: "toggle", key: "showPagination", label: "field.showPagination" },
      { type: "toggle", key: "uppercaseTitle", label: "field.uppercaseTitle" },
      { type: "toggle", key: "uppercaseItems", label: "field.uppercaseItems" },
    ],
  },
];

const STORE_KEY = "mm-panorama-constructor-v1";

function clone(value) {
  return JSON.parse(JSON.stringify(value));
}

function formatNum(n) {
  const rounded = Math.round(Number(n) * 1000) / 1000;
  return String(rounded);
}

function intIn(n, min, max, fallback) {
  n = Math.round(Number(n));
  if (!Number.isFinite(n)) return fallback;
  return Math.min(max, Math.max(min, n));
}

function numIn(n, min, max, fallback) {
  n = Number(n);
  if (!Number.isFinite(n)) return fallback;
  return Math.min(max, Math.max(min, n));
}

function isColor(value) {
  return typeof value === "string" && COLOR_RE.test(value.trim());
}

function safeName(name) {
  const clean = String(name || "Тема").replace(/[\r\n*/<>]/g, "").trim().slice(0, 48);
  return clean || "Тема";
}

function parseColor(value) {
  const v = String(value).trim();
  const hex = v.match(/^#([0-9a-fA-F]{6})$/);
  if (hex) {
    return {
      r: parseInt(hex[1].slice(0, 2), 16),
      g: parseInt(hex[1].slice(2, 4), 16),
      b: parseInt(hex[1].slice(4, 6), 16),
      a: 1,
      hex: "#" + hex[1].toLowerCase(),
    };
  }
  const rgba = v.match(/^rgba\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*([0-9.]+)\s*\)$/i);
  if (!rgba) return null;
  const r = +rgba[1];
  const g = +rgba[2];
  const b = +rgba[3];
  return { r, g, b, a: +rgba[4], hex: toHex(r, g, b) };
}

function toHex(r, g, b) {
  return "#" + [r, g, b].map((n) => Math.max(0, Math.min(255, n | 0)).toString(16).padStart(2, "0")).join("");
}

function formatRgba(r, g, b, a) {
  const alpha = (Math.round(Math.max(0, Math.min(1, a)) * 100) / 100).toFixed(2);
  return `rgba(${r | 0}, ${g | 0}, ${b | 0}, ${alpha})`;
}

function cleanTheme(input) {
  const theme = clone(DEFAULTS);
  if (!input || typeof input !== "object") return theme;
  for (const key of Object.keys(DEFAULTS)) {
    const value = input[key];
    if (typeof DEFAULTS[key] === "boolean") {
      if (typeof value === "boolean") theme[key] = value;
      continue;
    }
    if (typeof DEFAULTS[key] === "number") {
      const field = GROUPS.flatMap((group) => group.fields).find((item) => item.key === key);
      theme[key] = field
        ? (field.step < 1 ? numIn(value, field.min, field.max, DEFAULTS[key]) : intIn(value, field.min, field.max, DEFAULTS[key]))
        : DEFAULTS[key];
      continue;
    }
    if (key === "name") {
      theme.name = safeName(value);
      continue;
    }
    if (isColor(value)) theme[key] = String(value).trim();
  }
  return theme;
}

function sub(css, from, to, warnings) {
  if (from === to) return css;
  if (!css.includes(from)) {
    warnings.push(from);
    return css;
  }
  return css.split(from).join(to);
}

function buildCss(themeInput, base) {
  const t = cleanTheme(themeInput);
  const warnings = [];
  let css = String(base || "").replace(/^\uFEFF/, "").replace(/\r\n/g, "\n");
  const put = (from, to) => {
    css = sub(css, from, to, warnings);
  };

  put("@define accent: #FFA9DE;", `@define accent: ${t.accent};`);
  put("@define accent-soft: rgba(96, 128, 255, 0.40);", `@define accent-soft: ${t.accentSoft};`);
  put("@define frame: rgba(140, 170, 210, 0.22);", `@define frame: ${t.frame};`);
  put("@define dur: 0.15s;", `@define dur: ${formatNum(t.anim)}s;`);
  put("@define btn-bg: rgba(255, 255, 255, 0.05);", `@define btn-bg: ${t.btnBg};`);
  put("@define btn-bg-hover: rgba(255, 255, 255, 0.11);", `@define btn-bg-hover: ${t.btnHover};`);
  put("@define btn-bg-active: rgba(255, 255, 255, 0.18);", `@define btn-bg-active: ${t.btnActive};`);
  put("@define line-color: rgba(217, 217, 217, 0.04);", `@define line-color: ${t.line};`);
  put("@define label-color: #8b8f94;", `@define label-color: ${t.label};`);
  put("@define ntf-ok: #57e08a;", `@define ntf-ok: ${t.ntfOk};`);
  put("@define ntf-warn: #ffcf5c;", `@define ntf-warn: ${t.ntfWarn};`);
  put("@define ntf-err: #ff5f6d;", `@define ntf-err: ${t.ntfErr};`);

  const labelFrom = "\tfont-size: 17px;\n\tletter-spacing: 1px;\n\ttext-transform: uppercase;\n\tcolor: #cdd8e4;";
  const labelTo =
    `\tfont-size: ${t.itemSize}px;\n\tletter-spacing: 1px;\n\ttext-transform: ${t.uppercaseItems ? "uppercase" : "none"};\n\tcolor: #cdd8e4;`;
  put(labelFrom, labelTo);

  put("#cdd8e4", t.itemText);
  put("#8ea2b4", t.itemValue);
  put("#9fb0be", t.icon);
  put("#8b929a", t.arrow);
  put("#aab6c4", t.disabledText);
  put("rgba(6, 10, 16, 0.62)", t.disabledBg);
  put("rgba(110, 125, 140, 0.30)", t.disabledBorder);
  put("rgba(34, 34, 36, 0.99)", t.selectBg);
  put("rgba(255, 255, 255, 0.06)", t.selectBorder);
  put("rgba(70, 70, 74, 0.9)", t.optHover);
  put("rgba(90, 90, 96, 0.95)", t.optActive);
  put("background-color: rgba(0, 0, 0, 0.45);", `background-color: ${t.switchOff};`);
  put("background-color: #cfd4d9;", `background-color: ${t.knob};`);
  put("\tcolor: #cfd4d9;", `\tcolor: ${t.optText};`);

  const menuGrad = (top, bottom) =>
    `\tbackground-color: gradient( linear, 0% 0%, 0% 100%, from( ${top} ), to( ${bottom} ) );`;
  put(menuGrad(DEFAULTS.bgTop, DEFAULTS.bgBottom), menuGrad(t.bgTop, t.bgBottom));
  put(menuGrad(DEFAULTS.ntfBgTop, DEFAULTS.ntfBgBottom), menuGrad(t.ntfBgTop, t.ntfBgBottom));

  const titleFrom = "\tfont-size: 24px;\n\tletter-spacing: 3px;\n\ttext-transform: uppercase;\n\tcolor: accent;";
  const titleTo =
    `\tfont-size: ${t.titleSize}px;\n\tletter-spacing: ${t.titleTracking}px;\n\ttext-transform: ${t.uppercaseTitle ? "uppercase" : "none"};\n\tcolor: ${t.titleLinked ? "accent" : t.titleColor};`;
  put(titleFrom, titleTo);

  const descFrom = "\tfont-size: 16px;\n\tletter-spacing: 1px;\n\tcolor: label-color;\n\topacity: 0.9;";
  const descTo = `\tfont-size: ${t.descSize}px;\n\tletter-spacing: 1px;\n\tcolor: label-color;\n\topacity: 0.9;`;
  put(descFrom, descTo);

  put(".utils-menu\n{\n\twidth: 400px;", `.utils-menu\n{\n\twidth: ${t.menuWidth}px;`);
  put("\twidth: 396px;\n\theight: fit-children;", `\twidth: ${t.menuWidth - 4}px;\n\theight: fit-children;`);

  const menuRadiusFrom =
    "\ttransform: scale3d(1, 1, 1);\n\ttransition-property: opacity, transform;\n\ttransition-duration: dur;\n\ttransition-timing-function: ease-out;\n\tborder-radius: 4px;";
  const menuRadiusTo = menuRadiusFrom.replace("border-radius: 4px;", `border-radius: ${t.menuRadius}px;`);
  put(menuRadiusFrom, menuRadiusTo);

  const dotOpacity = t.showDots ? t.dotOpacity : 0;
  const blurCss = t.blur <= 0 ? "none" : `gaussian(${t.blur}, ${t.blur}, ${t.blur})`;
  const bgFrom =
    "\tbackground-repeat: repeat;\n\tbackground-img-opacity: 0.03;\n\tborder-radius: 4px;\n\tworld-blur: gaussian(2, 2, 2);";
  const bgTo =
    `\tbackground-repeat: repeat;\n\tbackground-img-opacity: ${formatNum(dotOpacity)};\n\tborder-radius: ${t.menuRadius}px;\n\tworld-blur: ${blurCss};`;
  put(bgFrom, bgTo);
  if (!t.showDots) {
    put(
      '\tbackground-image: url("s2r://panorama/images/backgrounds/bluedots_large_png.vtex");',
      "\tbackground-image: none;"
    );
  }

  if (t.btnRadius !== 5) {
    put("border-radius: 5px", `border-radius: ${t.btnRadius}px`);
    const optFrom =
      "\theight: 30px;\n\tflow-children: none;\n\tbackground-color: none;\n\tborder: 0px solid transparent;\n\tborder-radius: 4px;";
    put(optFrom, optFrom.replace("border-radius: 4px;", `border-radius: ${t.btnRadius}px;`));
  }

  if (t.itemHeight !== 40) {
    put("height: 40px", `height: ${t.itemHeight}px`);
    put("height: 288px", `height: ${6 * (t.itemHeight + 8)}px`);
    put("height: 178px", `height: ${t.itemHeight + 138}px`);
    put("y: 46px", `y: ${t.itemHeight + 6}px`);
    put("margin-bottom: 46px", `margin-bottom: ${t.itemHeight + 6}px`);
  }

  if (t.barWidth !== 4) put("width: 4px", `width: ${t.barWidth}px`);

  const knobOnFrom =
    "\thorizontal-align: right;\n\tmargin-left: 0px;\n\tmargin-right: 3px;\n\tbackground-color: #ffffff;";
  put(knobOnFrom, knobOnFrom.replace("#ffffff", t.knobOn));

  if (!t.switchOnLinked) {
    put(
      ".utils-item.is-on .utils-switch\n{\n\tbackground-color: accent;\n}",
      `.utils-item.is-on .utils-switch\n{\n\tbackground-color: ${t.switchOn};\n}`
    );
  }

  const extra = [];
  if (!t.leftBar) extra.push(".utils-bar-left { visibility: collapse; }");
  if (!t.rightBar) extra.push(".utils-bar-right { visibility: collapse; }");
  if (!t.showDesc) extra.push(".utils-desc { visibility: collapse; }");
  if (!t.showPagination) extra.push(".utils-pagination { visibility: collapse; }");
  if (extra.length) css += "\n" + extra.join("\n") + "\n";

  const header = `/* MenuManager panorama theme: ${safeName(t.name)} */\n`;
  return { css: header + css, warnings };
}

function crc32(bytes) {
  let c = ~0;
  for (let i = 0; i < bytes.length; i++) {
    c ^= bytes[i];
    for (let k = 0; k < 8; k++) c = (c >>> 1) ^ (0xedb88320 & -(c & 1));
  }
  return ~c >>> 0;
}

function u16(n) {
  return Uint8Array.of(n & 255, (n >> 8) & 255);
}

function u32(n) {
  return Uint8Array.of(n & 255, (n >> 8) & 255, (n >> 16) & 255, (n >> 24) & 255);
}

function concat(parts) {
  const out = new Uint8Array(parts.reduce((sum, part) => sum + part.length, 0));
  let offset = 0;
  for (const part of parts) {
    out.set(part, offset);
    offset += part.length;
  }
  return out;
}

function makeZip(files) {
  const encoder = new TextEncoder();
  const locals = [];
  const centrals = [];
  let offset = 0;
  for (const file of files) {
    const name = encoder.encode(file.name);
    const data = encoder.encode(file.text);
    const crc = crc32(data);
    const local = concat([
      u32(0x04034b50), u16(20), u16(0x0800), u16(0), u16(0), u16(0),
      u32(crc), u32(data.length), u32(data.length), u16(name.length), u16(0),
      name, data,
    ]);
    locals.push(local);
    centrals.push(concat([
      u32(0x02014b50), u16(20), u16(20), u16(0x0800), u16(0), u16(0), u16(0),
      u32(crc), u32(data.length), u32(data.length), u16(name.length), u16(0), u16(0),
      u16(0), u16(0), u32(0), u32(offset), name,
    ]));
    offset += local.length;
  }
  const central = concat(centrals);
  const end = concat([
    u32(0x06054b50), u16(0), u16(0), u16(files.length), u16(files.length),
    u32(central.length), u32(offset), u16(0),
  ]);
  return concat([...locals, central, end]);
}

function infoText(name) {
  const theme = safeName(name);
  return [
    `Theme / Тема: ${theme}`,
    "Built with the MenuManager panorama constructor.",
    "",
    "EN:",
    "Put the archive contents at the root of the client addon (MultiAddonManager / csgo_addons).",
    "Do not drop these files loose on the dedicated server.",
    "",
    "Files:",
    "  panorama/layout/custom_game/menu_ui.xml",
    "  panorama/styles/custom_game/ui.css",
    "",
    "In MenuManagerCore.json:",
    '  "DefaultMenu": "PanoramaMenu"',
    "",
    "After replacing the files, rebuild the addon and change the map.",
    "If your addon already has a custom menu_ui.xml, replace only ui.css.",
    "",
    "RU:",
    "Положи содержимое архива в корень клиентского аддона (MultiAddonManager / csgo_addons).",
    "Не клади эти файлы россыпью на dedicated server.",
    "",
    "Файлы:",
    "  panorama/layout/custom_game/menu_ui.xml",
    "  panorama/styles/custom_game/ui.css",
    "",
    "В MenuManagerCore.json:",
    '  "DefaultMenu": "PanoramaMenu"',
    "",
    "После замены пересобери аддон и смени карту.",
    "Если свой menu_ui.xml уже стоит в аддоне, можно заменить только ui.css.",
    "",
  ].join("\n");
}

globalThis.MMConstructor = { buildCss, DEFAULTS, PRESETS, makeZip, crc32, cleanTheme };

const state = {
  lang: typeof navigator !== "undefined" && /^en/i.test(navigator.language || "") ? "en" : "ru",
  theme: clone(DEFAULTS),
  preset: "sakura",
  preview: {
    title: "Админ",
    desc: "Управление сервером",
    page: 0,
    open: null,
    ntf: "success",
    showNtf: true,
  },
};

function tr(key) {
  const pack = I18N[state.lang] || I18N.ru;
  return pack[key] ?? I18N.ru[key] ?? key;
}

function entryValue(entry) {
  if (!entry.value) return "";
  return entry.raw ? entry.value : tr(entry.value);
}

function localizeStockCopy() {
  for (const preset of PRESETS) {
    const key = "preset." + preset.id;
    if (state.theme.name === I18N.ru[key] || state.theme.name === I18N.en[key]) {
      state.theme.name = tr(key);
      break;
    }
  }
  const titles = [I18N.ru["preview.title"], I18N.en["preview.title"]];
  const descs = [I18N.ru["preview.desc"], I18N.en["preview.desc"]];
  if (titles.includes(state.preview.title)) state.preview.title = tr("preview.title");
  if (descs.includes(state.preview.desc)) state.preview.desc = tr("preview.desc");
}

function applyI18n() {
  document.documentElement.lang = state.lang;
  document.title = tr("meta.title");
  document.querySelectorAll("[data-i18n]").forEach((el) => {
    el.textContent = tr(el.dataset.i18n);
  });
  document.querySelectorAll("[data-i18n-html]").forEach((el) => {
    el.innerHTML = tr(el.dataset.i18nHtml);
  });
  document.querySelectorAll("[data-i18n-aria]").forEach((el) => {
    el.setAttribute("aria-label", tr(el.dataset.i18nAria));
  });
  document.querySelectorAll("[data-lang]").forEach((button) => {
    button.setAttribute("aria-pressed", String(button.dataset.lang === state.lang));
  });
}

function $(id) {
  return document.getElementById(id);
}

function save() {
  try {
    localStorage.setItem(STORE_KEY, JSON.stringify({ lang: state.lang, theme: state.theme, preset: state.preset, preview: state.preview }));
  } catch {
    /* ignore private mode */
  }
}

function load() {
  try {
    const raw = localStorage.getItem(STORE_KEY);
    if (!raw) return;
    const data = JSON.parse(raw);
    if (data.lang === "en" || data.lang === "ru") state.lang = data.lang;
    state.theme = cleanTheme(data.theme);
    state.preset = PRESETS.some((preset) => preset.id === data.preset) ? data.preset : null;
    if (data.preview && typeof data.preview === "object") {
      state.preview.title = String(data.preview.title || state.preview.title).slice(0, 40);
      state.preview.desc = String(data.preview.desc || "").slice(0, 80);
      state.preview.page = data.preview.page === 1 ? 1 : 0;
      state.preview.ntf = NTF[data.preview.ntf] ? data.preview.ntf : "success";
      state.preview.showNtf = data.preview.showNtf !== false;
    }
  } catch {
    state.theme = clone(DEFAULTS);
  }
}

function applyPreview() {
  const t = state.theme;
  const root = document.documentElement;
  const set = (name, value) => root.style.setProperty(name, value);
  set("--mm-accent", t.accent);
  set("--mm-accent-soft", t.accentSoft);
  set("--mm-title", t.titleLinked ? t.accent : t.titleColor);
  set("--mm-bg-top", t.bgTop);
  set("--mm-bg-bottom", t.bgBottom);
  set("--mm-btn", t.btnBg);
  set("--mm-btn-hover", t.btnHover);
  set("--mm-btn-active", t.btnActive);
  set("--mm-label", t.label);
  set("--mm-item", t.itemText);
  set("--mm-value", t.itemValue);
  set("--mm-icon", t.icon);
  set("--mm-arrow", t.arrow);
  set("--mm-line", t.line);
  set("--mm-dis-bg", t.disabledBg);
  set("--mm-dis-border", t.disabledBorder);
  set("--mm-dis-text", t.disabledText);
  set("--mm-switch-off", t.switchOff);
  set("--mm-knob", t.knob);
  set("--mm-knob-on", t.knobOn);
  set("--mm-switch-on", t.switchOnLinked ? t.accent : t.switchOn);
  set("--mm-select-bg", t.selectBg);
  set("--mm-select-border", t.selectBorder);
  set("--mm-opt", t.optText);
  set("--mm-opt-hover", t.optHover);
  set("--mm-opt-active", t.optActive);
  set("--mm-frame", t.frame);
  set("--mm-ok", t.ntfOk);
  set("--mm-warn", t.ntfWarn);
  set("--mm-err", t.ntfErr);
  set("--mm-ntf-top", t.ntfBgTop);
  set("--mm-ntf-bottom", t.ntfBgBottom);
  set("--mm-width", t.menuWidth + "px");
  set("--mm-radius", t.menuRadius + "px");
  set("--mm-btn-radius", t.btnRadius + "px");
  set("--mm-title-size", t.titleSize + "px");
  set("--mm-title-track", t.titleTracking + "px");
  set("--mm-item-size", t.itemSize + "px");
  set("--mm-desc-size", t.descSize + "px");
  set("--mm-item-h", t.itemHeight + "px");
  set("--mm-bar", t.barWidth + "px");
  set("--mm-dur", t.anim + "s");
  set("--mm-blur-px", (t.blur <= 0 ? 0 : t.blur * 3) + "px");
  set("--mm-dots-view", t.showDots ? String(Math.min(0.85, t.dotOpacity * 10)) : "0");
  set("--mm-tt-title", t.uppercaseTitle ? "uppercase" : "none");
  set("--mm-tt-item", t.uppercaseItems ? "uppercase" : "none");

  $("pmenu").classList.toggle("no-left", !t.leftBar);
  $("pmenu").classList.toggle("no-right", !t.rightBar);
  $("pmenu").classList.toggle("no-dots", !t.showDots);
  $("pmenu").classList.toggle("no-desc", !t.showDesc);
  $("pmenu").classList.toggle("no-pager", !t.showPagination);
  $("ptitle").textContent = state.preview.title || tr("preview.menu");
  $("pdesc").textContent = state.preview.desc;
  paintToast();
  fitPreview();
}

function paintToast() {
  const toast = $("toast");
  const data = NTF[state.preview.ntf];
  toast.className = "toast " + data.cls + (state.preview.showNtf ? "" : " is-hidden");
  toast.querySelector("strong").textContent = tr(data.title);
  toast.querySelector("em").textContent = tr(data.text);
  document.querySelectorAll("[data-ntf]").forEach((button) => {
    button.setAttribute("aria-pressed", String(button.dataset.ntf === state.preview.ntf));
  });
  $("prev-ntf").checked = state.preview.showNtf;
}

function fitPreview() {
  const stage = $("screen");
  const menu = $("pmenu");
  const avail = Math.max(280, stage.clientWidth - 48);
  const scale = Math.min(1, avail / state.theme.menuWidth);
  menu.style.transform = menu.classList.contains("is-out") ? "scaleX(0)" : `scale(${scale})`;
}

function renderItems() {
  const host = $("pitems");
  host.replaceChildren();
  const page = PAGES[state.preview.page];
  page.forEach((entry, index) => {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "item k-" + entry.kind;
    if (entry.disabled) button.classList.add("is-disabled");
    if (entry.kind === "toggle" && entry.on) button.classList.add("is-on");
    if (entry.kind === "select" && state.preview.open === index) {
      button.classList.add("is-open");
      if (index >= 3) button.classList.add("is-up");
    }
    const label = document.createElement("span");
    label.className = "ilabel";
    label.textContent = entry.label ? tr(entry.label) : "";
    const value = document.createElement("span");
    value.className = "ivalue";
    value.textContent = entryValue(entry);
    const chev = document.createElement("i");
    chev.className = "ichev";
    const sw = document.createElement("i");
    sw.className = "iswitch";
    sw.append(document.createElement("i"));
    const empty = document.createElement("i");
    empty.className = "iempty";
    button.append(label, value, chev, sw, empty);
    if (entry.kind === "submenu") {
      const tip = document.createElement("i");
      tip.className = "itip";
      tip.textContent = tr("tip.open");
      button.append(tip);
    }
    if (entry.kind === "empty") button.setAttribute("aria-label", tr("empty"));
    if (entry.kind === "select") {
      const list = document.createElement("span");
      list.className = "iopts";
      for (const option of entry.options) {
        const opt = document.createElement("button");
        opt.type = "button";
        opt.className = "iopt" + (option === entry.value ? " is-sel" : "");
        opt.dataset.opt = option;
        opt.textContent = tr(option);
        list.append(opt);
      }
      button.append(list);
    }
    button.addEventListener("click", (event) => onItemClick(event, entry, index));
    host.append(button);
  });
  $("ppage").textContent = `${state.preview.page + 1} / ${PAGES.length}`;
  $("pprev").disabled = state.preview.page === 0;
  $("pnext").disabled = state.preview.page === PAGES.length - 1;
}

function onItemClick(event, entry, index) {
  const option = event.target.closest("[data-opt]");
  if (option) {
    entry.value = option.dataset.opt;
    state.preview.open = null;
    renderItems();
    return;
  }
  if (entry.disabled || entry.kind === "empty" || entry.kind === "value") return;
  if (entry.kind === "toggle") {
    entry.on = !entry.on;
    renderItems();
    return;
  }
  if (entry.kind === "select") {
    state.preview.open = state.preview.open === index ? null : index;
    renderItems();
  }
}

function renderControls() {
  const root = $("controls");
  root.replaceChildren();
  const presets = document.createElement("div");
  presets.className = "presets";
  for (const preset of PRESETS) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "preset";
    button.dataset.preset = preset.id;
    button.setAttribute("aria-pressed", String(state.preset === preset.id));
    const swatch = document.createElement("i");
    swatch.style.background = preset.swatch;
    button.append(swatch, document.createTextNode(tr("preset." + preset.id)));
    button.addEventListener("click", () => usePreset(preset.id));
    presets.append(button);
  }
  root.append(presets);

  const name = document.createElement("label");
  name.className = "name-field";
  const nameSpan = document.createElement("span");
  nameSpan.textContent = tr("theme.name");
  const nameInput = document.createElement("input");
  nameInput.id = "theme-name";
  nameInput.type = "text";
  nameInput.maxLength = 48;
  nameInput.value = state.theme.name;
  nameInput.addEventListener("input", () => {
    state.theme.name = safeName(nameInput.value);
    state.preset = null;
    markPresets();
    save();
  });
  name.append(nameSpan, nameInput);
  root.append(name);

  for (const group of GROUPS) {
    const section = document.createElement("section");
    section.className = "group";
    const title = document.createElement("h2");
    title.textContent = tr(group.title);
    section.append(title);
    for (const field of group.fields) {
      section.append(field.type === "color" ? colorRow(field) : field.type === "range" ? rangeRow(field) : switchRow(field));
    }
    root.append(section);
  }
  syncControls();
}

function colorRow(field) {
  const row = document.createElement("div");
  row.className = "crow" + (field.alpha ? "" : " no-alpha");
  row.dataset.key = field.key;
  const picker = document.createElement("input");
  picker.type = "color";
  picker.dataset.part = "rgb";
  const chip = document.createElement("i");
  chip.className = "chip";
  const paint = document.createElement("i");
  chip.append(paint);
  const meta = document.createElement("div");
  meta.className = "meta";
  const span = document.createElement("span");
  span.textContent = tr(field.label);
  const text = document.createElement("input");
  text.type = "text";
  text.spellcheck = false;
  text.dataset.part = "text";
  meta.append(span, text);
  row.append(picker, chip, meta);
  if (field.alpha) {
    const alpha = document.createElement("input");
    alpha.type = "range";
    alpha.min = "0";
    alpha.max = "100";
    alpha.dataset.part = "alpha";
    row.append(alpha);
  }
  row.addEventListener("input", (event) => onColorInput(field, event));
  return row;
}

function rangeRow(field) {
  const row = document.createElement("label");
  row.className = "range-field";
  row.dataset.key = field.key;
  const span = document.createElement("span");
  span.textContent = tr(field.label);
  const value = document.createElement("b");
  const input = document.createElement("input");
  input.type = "range";
  input.min = String(field.min);
  input.max = String(field.max);
  input.step = String(field.step);
  input.dataset.unit = field.unit;
  input.addEventListener("input", () => {
    const next = field.step < 1 ? numIn(+input.value, field.min, field.max, DEFAULTS[field.key]) : intIn(+input.value, field.min, field.max, DEFAULTS[field.key]);
    state.theme[field.key] = next;
    state.preset = null;
    value.textContent = formatRange(field, next);
    markPresets();
    applyPreview();
    save();
  });
  row.append(span, value, input);
  return row;
}

function switchRow(field) {
  const row = document.createElement("div");
  row.className = "switch-row";
  const span = document.createElement("span");
  span.textContent = tr(field.label);
  const button = document.createElement("button");
  button.type = "button";
  button.className = "switch";
  button.dataset.key = field.key;
  button.setAttribute("aria-label", tr(field.label));
  button.append(document.createElement("i"));
  button.addEventListener("click", () => {
    const next = !state.theme[field.key];
    state.theme[field.key] = next;
    if (field.key === "titleLinked" && !next) state.theme.titleColor = state.theme.accent;
    if (field.key === "switchOnLinked" && !next) state.theme.switchOn = state.theme.accent;
    state.preset = null;
    markPresets();
    syncControls();
    applyPreview();
    save();
  });
  row.append(span, button);
  return row;
}

function onColorInput(field, event) {
  const row = event.currentTarget;
  const part = event.target.dataset.part;
  if (!part) return;
  const current = parseColor(state.theme[field.key]) || parseColor(DEFAULTS[field.key]);
  let next = state.theme[field.key];
  if (part === "rgb") {
    const picked = parseColor(event.target.value);
    next = field.alpha ? formatRgba(picked.r, picked.g, picked.b, current.a) : event.target.value.toLowerCase();
  } else if (part === "alpha") {
    next = formatRgba(current.r, current.g, current.b, +event.target.value / 100);
  } else if (isColor(event.target.value.trim())) {
    next = event.target.value.trim();
    event.target.classList.remove("bad");
  } else {
    event.target.classList.add("bad");
    return;
  }
  state.theme[field.key] = next;
  state.preset = null;
  markPresets();
  syncColorRow(row, field);
  applyPreview();
  save();
}

function formatRange(field, value) {
  if (field.key === "blur" && value === 0) return tr("unit.none");
  const suffix = field.unit === "sec" ? " " + tr("unit.sec") : field.unit ? " " + field.unit : "";
  if (field.step < 1) return formatNum(value) + suffix;
  return value + suffix;
}

function syncControls() {
  const name = $("theme-name");
  if (name && document.activeElement !== name) name.value = state.theme.name;
  for (const group of GROUPS) {
    for (const field of group.fields) {
      if (field.type === "color") syncColorRow(document.querySelector(`.crow[data-key="${field.key}"]`), field);
      if (field.type === "range") {
        const row = document.querySelector(`.range-field[data-key="${field.key}"]`);
        const input = row.querySelector("input");
        input.value = String(state.theme[field.key]);
        row.querySelector("b").textContent = formatRange(field, state.theme[field.key]);
        input.disabled = field.key === "dotOpacity" && !state.theme.showDots;
      }
      if (field.type === "toggle") {
        const button = document.querySelector(`.switch[data-key="${field.key}"]`);
        button.setAttribute("aria-pressed", String(!!state.theme[field.key]));
      }
    }
  }
  markPresets();
}

function syncColorRow(row, field) {
  if (!row) return;
  const parsed = parseColor(state.theme[field.key]) || parseColor(DEFAULTS[field.key]);
  const picker = row.querySelector('[data-part="rgb"]');
  const text = row.querySelector('[data-part="text"]');
  picker.value = parsed.hex;
  if (document.activeElement !== text) text.value = state.theme[field.key];
  text.classList.toggle("bad", !isColor(state.theme[field.key]));
  row.querySelector(".chip i").style.background = state.theme[field.key];
  const alpha = row.querySelector('[data-part="alpha"]');
  if (alpha) alpha.value = String(Math.round(parsed.a * 100));
  const disabled = field.off && state.theme[field.off];
  row.classList.toggle("is-off", !!disabled);
  picker.disabled = !!disabled;
  text.disabled = !!disabled;
  if (alpha) alpha.disabled = !!disabled;
}

function markPresets() {
  document.querySelectorAll("[data-preset]").forEach((button) => {
    button.setAttribute("aria-pressed", String(button.dataset.preset === state.preset));
  });
}

function usePreset(id) {
  const preset = PRESETS.find((item) => item.id === id);
  state.theme = cleanTheme({ ...clone(DEFAULTS), ...preset.theme, name: tr("preset." + preset.id) });
  state.preset = id;
  syncControls();
  applyPreview();
  save();
}

function resetTheme() {
  usePreset("sakura");
}

function download(filename, bytes, type) {
  const blob = new Blob([bytes], { type });
  const link = document.createElement("a");
  link.href = URL.createObjectURL(blob);
  link.download = filename;
  document.body.append(link);
  link.click();
  link.remove();
  setTimeout(() => URL.revokeObjectURL(link.href), 1500);
}

function currentCss() {
  if (!globalThis.MM_BASE_CSS) throw new Error("Нет базового ui.css");
  return buildCss(state.theme, globalThis.MM_BASE_CSS);
}

function downloadCss() {
  const built = currentCss();
  if (built.warnings.length) console.warn(built.warnings);
  download("ui.css", built.css, "text/css");
}

function downloadZip() {
  const built = currentCss();
  if (built.warnings.length) console.warn(built.warnings);
  const zip = makeZip([
    { name: "panorama/styles/custom_game/ui.css", text: built.css },
    { name: "panorama/layout/custom_game/menu_ui.xml", text: globalThis.MM_BASE_XML || "" },
    { name: "info.txt", text: infoText(state.theme.name) },
  ]);
  download("custom_menu.zip", zip, "application/zip");
  $("install").hidden = false;
}

function downloadJson() {
  const text = JSON.stringify({ version: 1, theme: state.theme }, null, 2);
  download((safeName(state.theme.name) || "theme") + ".json", text, "application/json");
}

function importJson(file) {
  const reader = new FileReader();
  reader.onload = () => {
    try {
      const data = JSON.parse(String(reader.result));
      state.theme = cleanTheme(data.theme || data);
      state.preset = null;
      syncControls();
      applyPreview();
      save();
    } catch {
      $("theme-name").value = state.theme.name;
      alert(tr("import.error"));
    }
  };
  reader.readAsText(file);
}

function setLang(lang) {
  if (lang !== "ru" && lang !== "en" || lang === state.lang) return;
  state.lang = lang;
  localizeStockCopy();
  applyI18n();
  renderControls();
  renderItems();
  $("prev-title").value = state.preview.title;
  $("prev-desc").value = state.preview.desc;
  applyPreview();
  save();
}

function init() {
  load();
  localizeStockCopy();
  applyI18n();
  $("prev-title").value = state.preview.title;
  $("prev-desc").value = state.preview.desc;
  renderControls();
  renderItems();
  applyPreview();
  document.querySelectorAll("[data-lang]").forEach((button) => {
    button.addEventListener("click", () => setLang(button.dataset.lang));
  });

  $("prev-title").addEventListener("input", () => {
    state.preview.title = $("prev-title").value.slice(0, 40);
    applyPreview();
    save();
  });
  $("prev-desc").addEventListener("input", () => {
    state.preview.desc = $("prev-desc").value.slice(0, 80);
    applyPreview();
    save();
  });
  $("prev-ntf").addEventListener("change", () => {
    state.preview.showNtf = $("prev-ntf").checked;
    paintToast();
    save();
  });
  document.querySelectorAll("[data-ntf]").forEach((button) => {
    button.addEventListener("click", () => {
      state.preview.ntf = button.dataset.ntf;
      state.preview.showNtf = true;
      paintToast();
      save();
    });
  });
  $("pprev").addEventListener("click", () => {
    if (state.preview.page > 0) {
      state.preview.page -= 1;
      state.preview.open = null;
      renderItems();
      save();
    }
  });
  $("pnext").addEventListener("click", () => {
    if (state.preview.page < PAGES.length - 1) {
      state.preview.page += 1;
      state.preview.open = null;
      renderItems();
      save();
    }
  });
  $("pclose").addEventListener("click", () => {
    const menu = $("pmenu");
    menu.classList.add("is-out");
    menu.style.transform = "scaleX(0)";
    setTimeout(() => {
      menu.classList.remove("is-out");
      fitPreview();
    }, 420);
  });
  $("dl-zip").addEventListener("click", downloadZip);
  $("dl-css").addEventListener("click", downloadCss);
  $("dl-json").addEventListener("click", downloadJson);
  $("reset").addEventListener("click", resetTheme);
  $("import").addEventListener("click", () => $("import-file").click());
  $("import-file").addEventListener("change", () => {
    const file = $("import-file").files && $("import-file").files[0];
    if (file) importJson(file);
    $("import-file").value = "";
  });
  $("install-close").addEventListener("click", () => {
    $("install").hidden = true;
  });
  $("install").addEventListener("click", (event) => {
    if (event.target === $("install")) $("install").hidden = true;
  });
  window.addEventListener("resize", fitPreview);
}

if (typeof document !== "undefined" && document.getElementById) {
  document.addEventListener("DOMContentLoaded", init);
}
