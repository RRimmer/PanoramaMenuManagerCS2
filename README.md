# PanoramaMenuManagerCS2

Форк [MenuManagerCS2](https://github.com/NickFox007/MenuManagerCS2) от **E!N**. Эта сборка продолжает форк [Stimayk](https://github.com/Stimayk/MenuManagerCS2).

Панорамное меню для Counter-Strike 2: кликабельный диалог, слайдеры, выпадающие списки и уведомления. Другие плагины открывают меню через `MenuManagerApi`.

- Сайт и конструктор вида: [genesis-cs.space/menuconstructor](https://genesis-cs.space/menuconstructor/index.html)
- API для разработчиков: [menumanager/api](https://genesis-cs.space/menuconstructor/menumanager/api/index.html)
- Список изменений: [menumanager/changelog](https://genesis-cs.space/menuconstructor/menumanager/changelog/index.html)
- Страница на CSDevs: [menumanager.1174](https://csdevs.net/resources/menumanager.1174/)
- Оригинальный ресурс: [menumanager.726](https://csdevs.net/resources/menumanager.726/)

Текущая версия плагина: **v1.1.10**.

## Что лежит в репозитории

| Папка | Зачем |
| --- | --- |
| `MenuManagerCore` | Плагин сервера |
| `MenuManagerApi` | Библиотека для чужих плагинов |
| `MenuManagerTest` | Короткий пример |
| `custom_menu` | Клиентская панорама: `menu_ui.xml` и `ui.css` |
| `menu-constructor` | Сайт конструктора, API и список изменений |

Готовый архив для сервера в git не входит. Его нужно прикрепить к [Release](https://github.com/RRimmer/PanoramaMenuManagerCS2/releases) вручную.

## Установка с Release

В архиве две папки.

`Server-plugins` копируется в `game/csgo/addons/`. Получится:

- `addons/counterstrikesharp/plugins/MenuManagerCore/`
- `addons/counterstrikesharp/shared/MenuManagerApi/`
- `addons/counterstrikesharp/configs/plugins/MenuManagerCore/MenuManagerCore.json`

`MenuManagerApi.dll` должна быть только в `shared/MenuManagerApi/`. Копии в `plugins/MenuManagerCore`, `plugins/IksAdmin` и `plugins/VIPCore` ломают меню. После замены DLL нужен полный перезапуск сервера. `css_plugins reload` старую сборку не выгружает.

`Content-addonmanager` копируется в корень клиентского аддона MultiAddonManager, не россыпью на dedicated server. После замены панорамы игрок заходит на сервер заново.

В конфиге оставь `"DefaultMenu": "PanoramaMenu"`. Уведомления включены полем `"Notifications": true`. Игрок с типом Панорама меняет их себе в `!menu`.

## Сборка

Нужен .NET 10 SDK и пакет CounterStrikeSharp.API.

```bash
dotnet build MenuManager.sln --configuration Release
```

`MenuManagerApi.dll` кладётся в `shared/MenuManagerApi/`. В папку плагина её не копируй.

## Лицензия

[GNU GPL v3](LICENSE). Форк сохраняет свободу оригинала [Nick Fox](https://github.com/NickFox007/MenuManagerCS2).

---

# English

Fork of [MenuManagerCS2](https://github.com/NickFox007/MenuManagerCS2) by **E!N**. This build continues the [Stimayk](https://github.com/Stimayk/MenuManagerCS2) fork.

A panorama menu for Counter-Strike 2: a clickable dialog, switches, dropdowns and toasts. Other plugins open menus through `MenuManagerApi`.

Current plugin version: **v1.1.10**.

The player-ready archive is not stored in git. Attach it to a [Release](https://github.com/RRimmer/PanoramaMenuManagerCS2/releases) yourself.

`Server-plugins` goes into `game/csgo/addons/`. Keep `MenuManagerApi.dll` only in `shared/MenuManagerApi/`. Restart the server after replacing a DLL. `css_plugins reload` does not unload the old assembly.

`Content-addonmanager` goes into the MultiAddonManager client addon root. Players have to rejoin after a panorama update.

Keep `"DefaultMenu": "PanoramaMenu"`. Notifications stay on with `"Notifications": true`. A Panorama player changes them in `!menu`.

```bash
dotnet build MenuManager.sln --configuration Release
```
