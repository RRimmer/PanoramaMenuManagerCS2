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

Copy `Content-addonmanager` into the root of the MultiAddonManager client addon. Do not drop the files loose on the dedicated server. After a panorama update the player has to rejoin.

Keep `"DefaultMenu": "PanoramaMenu"` or `"PanoramaWasdMenu"` in the config. Empty `PanoramaPosition` means center; `left` and `right` sit in the vertical middle. `"HudOpenDelay": 0.2` pauses the normal panorama and CSGO Menu so a chat command and the HUD are not the same frame. `GetSelectedMenu` returns the real menu type, including WASD. Menu colors are chosen in the constructor and installed with the client addon. Notifications stay on with `"Notifications": true`. A player on Panorama changes them, and the panel position, for themselves in `!menu`.

You can read more on the site: https://genesis-cs.space/menuconstructor/index.html

**1.2.03.** `configs/plugins/MenuManagerCore/MenuManager_Modules_Translation.json` translates menu labels per plugin. CS2-SimpleAdmin and IksAdmin are already filled in. A missing file is created on the server; an existing file is left as it is. Linux `libe_sqlite3.so` now works on glibc 2.28, so an older host no longer fails with `GLIBC_2.33 not found`. On panorama, `AddToggle` and `AddSelect` update the row in place. The menu does not close and the page stays.

The SwiftlyS2 build is the release archive `MenuManagerCS2-1.2.03-swiftlys2.zip`. [PMM_WeaponPaints 0.1.0](https://github.com/RRimmer/PMM-WeaponPaints/releases/tag/PMM_WeaponPaints-0.1.0) is a separate project (beta). Its panorama design is inspired by [EliteGames.Ro](https://elitegames.ro).

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

`Content-addonmanager` копируется в корень клиентского аддона MultiAddonManager, не россыпью на dedicated server. После замены панорамы игрок заходит на сервер заново.

В конфиге можно `"DefaultMenu": "PanoramaMenu"` или `"PanoramaWasdMenu"`. Пустой `PanoramaPosition` — центр; `left` и `right` стоят по вертикали посередине. `"HudOpenDelay": 0.2` — пауза обычной панорамы и CSGO Menu, чтобы команда в чате и HUD не попадали в один кадр. `GetSelectedMenu` отдаёт реальный тип меню, включая WASD. Цвет меню выбирается в конструкторе и ставится вместе с клиентским аддоном. Уведомления включены полем `"Notifications": true`. Игрок с типом Панорама меняет их и позицию панели себе в `!menu`.

Вы можете ознакомиться с более подробной информацией на сайте: https://genesis-cs.space/menuconstructor/index.html

**1.2.03.** Файл `configs/plugins/MenuManagerCore/MenuManager_Modules_Translation.json` уже в репозитории: переводы меню CS2-SimpleAdmin и IksAdmin. Если на сервере файла нет, ядро создаёт этот пример. Уже существующий файл не перезаписывается. `libe_sqlite3.so` для Linux собран под glibc 2.28, старый хост больше не падает с `GLIBC_2.33 not found`. На панораме `AddToggle` и `AddSelect` обновляют строку на месте: меню не закрывается и страница не сбрасывается.

Сборка для SwiftlyS2 — архив релиза `MenuManagerCS2-1.2.03-swiftlys2.zip`. [PMM_WeaponPaints 0.1.0](https://github.com/RRimmer/PMM-WeaponPaints/releases/tag/PMM_WeaponPaints-0.1.0) — отдельный проект (beta). Дизайн его панорамы вдохновлён [EliteGames.Ro](https://elitegames.ro).

`PisexMenusBridge` не подменяет `IMenusApi` Pisex. Плагины, собранные с cs2-menus, по-прежнему зовут ту библиотеку. Если мост включён и Pisex рисует тот же `menu_ui.xml`, этот плагин накладывает на их HUD позицию и подписи открытого списка. Меню без этого layout остаются интерфейсом Pisex. Отсутствие cs2-menus не ошибка загрузки.

## Сборка

Нужен .NET 10 SDK и пакет CounterStrikeSharp.API.

```bash
dotnet build MenuManager.sln --configuration Release
```

`MenuManagerApi.dll` кладётся в `shared/MenuManagerApi/`. В папку плагина её не копируй.

## Лицензия

[GNU GPL v3](LICENSE). Форк сохраняет свободу оригинала [Nick Fox](https://github.com/NickFox007/MenuManagerCS2).
