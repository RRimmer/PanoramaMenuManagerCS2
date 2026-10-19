# PanoramaMenuManagerCS2

<h2><a href="https://genesis-cs.space/menuconstructor/index.html">>>>Более подробная информация на сайте<<<</a></h2>

Fork of [MenuManagerCS2](https://github.com/NickFox007/MenuManagerCS2) by **Rimmer**. This build continues the [Stimayk](https://github.com/Stimayk/MenuManagerCS2) fork.

A panorama menu for Counter-Strike 2: a clickable dialog or the same dialog on WASD, switches, dropdowns and toasts. Other plugins open menus through `MenuManagerApi`.

Current plugin version: ![Version](https://img.shields.io/github/v/release/RRimmer/PanoramaMenuManagerCS2?label=version&color=blue)

## Constructor site

This site is ours. **Rimmer** built it for this fork: a live look editor, an API reference and a changelog. The original MenuManager does not ship this site.

**[genesis-cs.space/menuconstructor](https://genesis-cs.space/menuconstructor/index.html)**

- [Constructor](https://genesis-cs.space/menuconstructor/index.html) — colors, sizes, switches, the dropdown and where the panel sits (left, center, right). The menu updates as you edit, and you can download your panorama: `ui.css` and `menu_ui.xml`.
- [API](https://genesis-cs.space/menuconstructor/menumanager/api/index.html) — how a plugin opens a menu, a switch, a list and a toast. With C# examples.
- [Changelog](https://genesis-cs.space/menuconstructor/menumanager/changelog/index.html) — core, site and panorama.
- [Supported plugins](https://genesis-cs.space/menuconstructor/menumanager/plugins/index.html) — VIP, LR and other builds that already work with this menu.

## Repository layout

| Folder | What it is |
| --- | --- |
| `MenuManagerCore` | The server plugin |
| `MenuManagerApi` | The library other plugins call |
| `MenuManagerTest` | A short example |
| `custom_menu` | The client panorama: `menu_ui.xml` and `ui.css` |

The player-ready archive is not stored in git. Attach it to a [Release](https://github.com/RRimmer/PanoramaMenuManagerCS2/releases) yourself.

## Install from a Release

The archive has two folders.

Copy `Server-plugins` into `game/csgo/addons/`. You get:

- `addons/counterstrikesharp/plugins/MenuManagerCore/`
- `addons/counterstrikesharp/shared/MenuManagerApi/`
- `addons/counterstrikesharp/configs/plugins/MenuManagerCore/MenuManagerCore.json`

Keep `MenuManagerApi.dll` only in `shared/MenuManagerApi/`. Copies in `plugins/MenuManagerCore`, `plugins/IksAdmin` and `plugins/VIPCore` break menus. After replacing a DLL, restart the server. `css_plugins reload` does not unload the old assembly.

From 1.1.11, leave `plugins/MenuManagerCore/runtimes` in place. SQLite loads `libe_sqlite3.so` or `e_sqlite3.dll` from that folder and writes `menumanager.db` next to the plugin.

PlayerSettingsCS2 and AnyBaseLibCS2 are not required. This fork stores the player menu itself.

From 1.3.00 the release also has three empty stubs: `plugins/PlayerSettings/PlayerSettings.dll`, `shared/PlayerSettingsApi/PlayerSettingsApi.dll`, `shared/AnyBaseLib/AnyBaseLib.dll`. CS2-SimpleAdmin and other plugins made for NickFox's MenuManagerCS2 check these files and unload without them. The stubs have no `ISettingsApi`. If a plugin really calls `settings:nfcore`, install the original PlayerSettingsCS2 and AnyBaseLibCS2 instead and do not overwrite them with the stubs.

`"PanoramaLayoutPath"` (empty = `panorama/layout/custom_game/menu_ui.xml`) and `IMenuApi.SetPanoramaHudLayout(path)` switch the panorama layout file. It is one file for the whole server and must keep the ids and classes of `menu_ui.xml`.

Number keys (1.3.00): CSGO Menu and `PanoramaDigitMenu` ("Panorama (number keys)": the panorama without the mouse, items 1–6, 8 back, 9 next, 0 exit) are driven by the binds `css_1` … `css_0`. Choosing either in `!menu` opens the number keys test: keys 1…0 light up as the server gets them, the bind line goes to chat and console. The server cannot write binds into the client, the player pastes the line once:
`spec_usenumberkeys_nobinds 0;bind "1" "css_1; slot1";…;bind "0" "css_0; slot10"` (the space after `;` is required, it makes the keys work while dead too).
Other plugins can use the same keys: `IMenuApi.GetNumberKeysBindLine()`, `PrintNumberKeysBinds(player)`, `HasNumberKeysChecked(player)`, `OpenNumberKeysCheck(player, onDone)` and the event `NumberKeyPressed(player, digit)`. `"NumberKeysTestTimeout": 40` in `MenuManagerCore.json`.

Menu sounds (1.3.00): `SoundScroll`, `SoundClick`, `SoundDisabled`, `SoundBack`, `SoundNext` (empty = `SoundScroll`), `SoundExit` are sound event names. Own sounds go into the workshop addon (`sounds/pmm/*.wav` + `soundevents/soundevents_addon.vsndevts`, events `PMM.Menu.Select` / `Back` / `Next` / `Exit`); the server precaches the file through `"PrecacheResources": [ "soundevents/soundevents_addon.vsndevts" ]`. Plugins play the same sounds with `IMenuApi.PlayMenuSound(player, MenuSound.Click)` and read the names with `GetMenuSound(MenuSound)`.

From 1.2.04 the core also opens menus of plugins built for schwarper's CS2MenuManager 1.0.43, such as cs2-store. Do not install schwarper's CS2MenuManager: on start the core writes its own `shared/CS2MenuManager/CS2MenuManager.dll` (`"CS2MenuManagerCompat": true`). Update `shared/MenuManagerApi` together with the core.

Copy `Content-addonmanager` into the root of the MultiAddonManager client addon. Do not drop the files loose on the dedicated server. After a panorama update the player has to rejoin.

Keep `"DefaultMenu": "PanoramaMenu"` or `"PanoramaWasdMenu"` in the config. Empty `PanoramaPosition` means center; `left` and `right` sit in the vertical middle. Menu colors are chosen in the constructor and installed with the client addon. Notifications stay on with `"Notifications": true`. A player on Panorama changes them, and the panel position, for themselves in `!menu`.

`PisexMenusBridge` does not replace the Pisex `IMenusApi`. Plugins linked to cs2-menus still call that library. When the bridge is on and Pisex draws the same `menu_ui.xml`, this plugin overlays position and open-list labels on that HUD. Menus that do not use this layout stay on the Pisex UI. Missing cs2-menus is not a load error.

## Build

You need the .NET 10 SDK and the CounterStrikeSharp.API package.

```bash
dotnet build MenuManager.sln --configuration Release
```

Put `MenuManagerApi.dll` in `shared/MenuManagerApi/`. Do not copy it into the plugin folder.

## License

[GNU GPL v3](LICENSE). The fork keeps the freedoms of the original by [Nick Fox](https://github.com/NickFox007/MenuManagerCS2).

---

# PanoramaMenuManagerCS2

Форк [MenuManagerCS2](https://github.com/NickFox007/MenuManagerCS2) от **Rimmer**. Эта сборка продолжает форк [Stimayk](https://github.com/Stimayk/MenuManagerCS2).

Панорамное меню для Counter-Strike 2: кликабельный диалог или тот же диалог на WASD, слайдеры, выпадающие списки и уведомления. Другие плагины открывают меню через `MenuManagerApi`.

Текущая версия плагина: ![Version](https://img.shields.io/github/v/release/RRimmer/PanoramaMenuManagerCS2?label=version&color=blue)

## Сайт конструктора

Это наш сайт. Его сделал **Rimmer** для этого форка: живой конструктор внешнего вида, справочник API и список изменений. Оригинальный MenuManager такого сайта не даёт.

**[genesis-cs.space/menuconstructor](https://genesis-cs.space/menuconstructor/index.html)**

- [Конструктор](https://genesis-cs.space/menuconstructor/index.html) — цвета, размеры, слайдеры, выпадающий список и место панели (слева, центр, справа). Сразу видно меню и можно скачать свою панораму: `ui.css` и `menu_ui.xml`.
- [API](https://genesis-cs.space/menuconstructor/menumanager/api/index.html) — как плагину открыть меню, слайдер, список и тост. С примерами на C#.
- [Список изменений](https://genesis-cs.space/menuconstructor/menumanager/changelog/index.html) — ядро, сайт и панорама.
- [Поддерживаемые плагины](https://genesis-cs.space/menuconstructor/menumanager/plugins/index.html) — VIP, LR и другие сборки, которые уже работают с этим меню.

## Что лежит в репозитории

| Папка | Зачем |
| --- | --- |
| `MenuManagerCore` | Плагин сервера |
| `MenuManagerApi` | Библиотека для чужих плагинов |
| `MenuManagerTest` | Короткий пример |
| `custom_menu` | Клиентская панорама: `menu_ui.xml` и `ui.css` |

Готовый архив для сервера в git не входит. Его нужно прикрепить к [Release](https://github.com/RRimmer/PanoramaMenuManagerCS2/releases) вручную.

## Установка с Release

В архиве две папки.

`Server-plugins` копируется в `game/csgo/addons/`. Получится:

- `addons/counterstrikesharp/plugins/MenuManagerCore/`
- `addons/counterstrikesharp/shared/MenuManagerApi/`
- `addons/counterstrikesharp/configs/plugins/MenuManagerCore/MenuManagerCore.json`

`MenuManagerApi.dll` должна быть только в `shared/MenuManagerApi/`. Копии в `plugins/MenuManagerCore`, `plugins/IksAdmin` и `plugins/VIPCore` ломают меню. После замены DLL нужен полный перезапуск сервера. `css_plugins reload` старую сборку не выгружает.

С 1.1.11 папку `plugins/MenuManagerCore/runtimes` не удаляй. SQLite берёт оттуда `libe_sqlite3.so` или `e_sqlite3.dll` и пишет `menumanager.db` рядом с плагином.

PlayerSettingsCS2 и AnyBaseLibCS2 не нужны. Этот форк сам хранит меню игрока.

С 1.3.00 в релизе лежат три пустые заглушки: `plugins/PlayerSettings/PlayerSettings.dll`, `shared/PlayerSettingsApi/PlayerSettingsApi.dll`, `shared/AnyBaseLib/AnyBaseLib.dll`. CS2-SimpleAdmin и другие плагины под MenuManagerCS2 от NickFox проверяют эти файлы и без них выгружаются. `ISettingsApi` в заглушках нет. Если плагин реально вызывает `settings:nfcore`, поставь оригинальные PlayerSettingsCS2 и AnyBaseLibCS2 и не перезаписывай их заглушками.

`"PanoramaLayoutPath"` (пусто = `panorama/layout/custom_game/menu_ui.xml`) и `IMenuApi.SetPanoramaHudLayout(path)` меняют файл панорамы. Файл один на весь сервер и должен сохранить id и классы `menu_ui.xml`.

Цифры (1.3.00): CSGO Menu и `PanoramaDigitMenu` («Панорама (цифры)»: панорама без мыши, пункты 1–6, 8 назад, 9 далее, 0 выход) работают от биндов `css_1` … `css_0`. При выборе в `!menu` открывается проверка цифр: клавиши 1…0 загораются, когда сервер их получает, строка с биндами уходит в чат и консоль. Сервер не может прописать бинды игроку, игрок вставляет строку один раз:
`spec_usenumberkeys_nobinds 0;bind "1" "css_1; slot1";…;bind "0" "css_0; slot10"` (пробел после `;` обязателен, так цифры работают и мёртвым).
Другие плагины могут пользоваться теми же цифрами: `IMenuApi.GetNumberKeysBindLine()`, `PrintNumberKeysBinds(player)`, `HasNumberKeysChecked(player)`, `OpenNumberKeysCheck(player, onDone)` и событие `NumberKeyPressed(player, digit)`. `"NumberKeysTestTimeout": 40` в `MenuManagerCore.json`.

Звуки меню (1.3.00): `SoundScroll`, `SoundClick`, `SoundDisabled`, `SoundBack`, `SoundNext` (пусто = `SoundScroll`), `SoundExit` — имена звуковых событий. Свои звуки кладутся в аддон мастерской (`sounds/pmm/*.wav` + `soundevents/soundevents_addon.vsndevts`, события `PMM.Menu.Select` / `Back` / `Next` / `Exit`); сервер прекэширует файл через `"PrecacheResources": [ "soundevents/soundevents_addon.vsndevts" ]`. Плагины играют те же звуки через `IMenuApi.PlayMenuSound(player, MenuSound.Click)` и читают имена через `GetMenuSound(MenuSound)`.

`Content-addonmanager` копируется в корень клиентского аддона MultiAddonManager, не россыпью на dedicated server. После замены панорамы игрок заходит на сервер заново.

В конфиге можно `"DefaultMenu": "PanoramaMenu"` или `"PanoramaWasdMenu"`. Пустой `PanoramaPosition` — центр; `left` и `right` стоят по вертикали посередине. Цвет меню выбирается в конструкторе и ставится вместе с клиентским аддоном. Уведомления включены полем `"Notifications": true`. Игрок с типом Панорама меняет их и позицию панели себе в `!menu`.

`PisexMenusBridge` не подменяет `IMenusApi` Pisex. Плагины, собранные с cs2-menus, по-прежнему зовут ту библиотеку. Если мост включён и Pisex рисует тот же `menu_ui.xml`, этот плагин накладывает на их HUD позицию и подписи открытого списка. Меню без этого layout остаются интерфейсом Pisex. Отсутствие cs2-menus не ошибка загрузки.

## Сборка

Нужен .NET 10 SDK и пакет CounterStrikeSharp.API.

```bash
dotnet build MenuManager.sln --configuration Release
```

`MenuManagerApi.dll` кладётся в `shared/MenuManagerApi/`. В папку плагина её не копируй.

## Лицензия

[GNU GPL v3](LICENSE). Форк сохраняет свободу оригинала [Nick Fox](https://github.com/NickFox007/MenuManagerCS2).
