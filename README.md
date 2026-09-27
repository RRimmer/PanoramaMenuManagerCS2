# [FORK] MenuManagerCS2

[MenuManager main repository](https://github.com/NickFox007/MenuManagerCS2)

[CSDevs main](https://csdevs.net/resources/menumanager.726/)

[Original fork this build is based on](https://github.com/Stimayk/MenuManagerCS2)

MenuManagerCore — main core plugin  
MenuManagerApi — API library for other plugins  
MenuManagerTest — example plugin

Created with the support of [NovaHost](https://nova-hosting.ru?ref=ein)

More info: [CSDevs](https://csdevs.net/resources/menumanager.1174/)

---

## RU

### Что нового в 1.1.10

- Открытый выпадающий список больше не сдвигает свою строку и не растягивает её фон поверх соседних пунктов.
- Список рисуется поверх соседних строк. Прозрачная обёртка держит список, сама строка остаётся 40px.

### Что нового в 1.1.9

- Пункты выпадающего списка больше не копируют текущее значение строки. У каждого пункта свой текст.

### Что нового в 1.1.8

- Пункт со списком значений открывается выпадающим списком на месте, если значений не больше 12. Длиннее — как раньше, подменю.
- Значок уведомления рисуется панелью: галочка, восклицательный знак, крестик. Нужен новый клиентский аддон.
- После смены настройки панорама остаётся на той же странице, пока меню не закроют.

### Что нового в 1.1.7

- В `MenuManagerCore.json` поле `Notifications`, по умолчанию включено.
- В `!menu`, если выбран Panorama, пункт «Панорама»: игрок сам включает или выключает свои уведомления.
- Поставка одним архивом: `Server-plugins` и `Content-addonmanager`.

### Что нового в 1.1.6

- Слайдер и разбор «вкл/выкл» живут внутри MenuManagerCore. Старая `MenuManagerApi.dll` больше не роняет `!menu`, `!admin` и `!vip`.
- В архиве плагина нет второй `MenuManagerApi.dll`. На сервере оставь одну копию: `shared/MenuManagerApi/MenuManagerApi.dll`. После замены нужен полный перезапуск сервера, не `css_plugins reload`.

### Что нового в 1.1.5

- Панорама показывает уведомление: успех, внимание, ошибка. Плагины вызывают `Notify`.
- Пункт меню вида «название: вкл/выкл» (On/Off, Açık/Kapalı) рисуется слайдером. Явный вызов — `AddToggle`.
- В `menu_ui.xml` у значка уведомления есть id, без обновлённого `custom_menu.zip` значок пустой.

### Что нового в 1.1.4

- Панорамное меню больше не создаётся и не рисуется в том же кадре, что команда `!admin` / `css_admin`.
- Layout `custom_hud_layout` прогревается при старте карты.
- Отрисовка диалога и захват ввода сдвинуты на следующие кадры.
- Это убирает кик `NETWORK_DISCONNECT_OVERFLOW` (`ProcessMessages` / `CCLCMsg_Move` дольше ~200 мс), когда Valve рвёт канал из‑за нагрузки на тик.

### Что появилось в 1.1.3

- Тип меню `PanoramaMenu`: кликабельный диалог как у VIP, не HTML по центру экрана.
- В `MenuManagerCore.json` по умолчанию `"DefaultMenu": "PanoramaMenu"`.
- IksAdmin `MenuType` 3 (`ButtonMenu`) открывает меню по умолчанию (панораму), а не старое WASD/HTML.
- Нужны файлы из `custom_menu.zip` в workshop-аддоне (MultiAddonManager), не россыпью на dedicated server.
- `MenuManagerApi.dll` должна лежать в `addons/counterstrikesharp/shared/MenuManagerApi/`. Копии внутри `plugins/IksAdmin` и `plugins/MenuManagerCore` ломают меню (`Object reference not set`).

### Установка

1. `MenuManagerCore.zip` → `addons/counterstrikesharp/`
2. `MenuManagerApi.zip` → `addons/counterstrikesharp/shared/` (одна копия API)
3. `custom_menu.zip` → корень клиентского аддона (`panorama/layout/...`, `panorama/styles/...`)
4. `menu_buttons.zip` — только для старого WASD-меню, как в 1.1.2

---

## EN

### What is new in 1.1.10

- An open dropdown no longer shifts its row or stretches that row's background over the items around it.
- The list draws on top of the rows around it. A transparent wrapper holds the list. The row itself stays 40px.

### What is new in 1.1.9

- Dropdown entries no longer repeat the row's current value. Each entry has its own text.

### What is new in 1.1.8

- A row with a value list opens a dropdown in place when there are 12 values or fewer. Longer lists still open a submenu.
- The toast icon is drawn as a panel: check, exclamation, cross. Install the new client addon.
- After a setting change, panorama stays on the same page until the menu is closed.

### What is new in 1.1.7

- `MenuManagerCore.json` has `Notifications`, on by default.
- In `!menu`, when Panorama is selected, the Panorama item lets each player turn their own notifications on or off.
- One archive: `Server-plugins` and `Content-addonmanager`.

### What is new in 1.1.6

- The switch and the on/off row parsing live inside MenuManagerCore. An older `MenuManagerApi.dll` no longer takes down `!menu`, `!admin` and `!vip`.
- The plugin archive does not ship a second `MenuManagerApi.dll`. Keep one copy at `shared/MenuManagerApi/MenuManagerApi.dll`. After replacing it, restart the server. `css_plugins reload` keeps the old assembly loaded.

### What is new in 1.1.5

- Panorama shows a toast: success, warning, error. Other plugins call `Notify`.
- A menu row shaped like “name: on/off” (Вкл/Выкл, Açık/Kapalı) is drawn as a switch. Explicit call: `AddToggle`.
- `menu_ui.xml` gives the toast icon an id. Without the updated `custom_menu.zip` the icon stays empty.

### What is new in 1.1.4

- The panorama menu is no longer created or drawn on the same tick as `!admin` / `css_admin`.
- The `custom_hud_layout` entity is warmed up when the map starts.
- Dialog drawing and input capture are deferred to later frames.
- This avoids the `NETWORK_DISCONNECT_OVERFLOW` kick (`ProcessMessages` / `CCLCMsg_Move` taking longer than ~200 ms) when Valve drops the client because the tick stalled.

### What landed in 1.1.3

- New menu type `PanoramaMenu`: a clickable dialog like VIP, not center-screen HTML.
- `MenuManagerCore.json` defaults to `"DefaultMenu": "PanoramaMenu"`.
- IksAdmin `MenuType` 3 (`ButtonMenu`) uses the default menu (panorama), not the old WASD/HTML menu.
- Files from `custom_menu.zip` go into the workshop addon (MultiAddonManager), not as loose files on the dedicated server.
- Put `MenuManagerApi.dll` in `addons/counterstrikesharp/shared/MenuManagerApi/`. Extra copies inside `plugins/IksAdmin` or `plugins/MenuManagerCore` break the menu (`Object reference not set`).

### Install

1. Extract `MenuManagerCore.zip` into `addons/counterstrikesharp/`
2. Extract `MenuManagerApi.zip` into `addons/counterstrikesharp/shared/` (one API copy)
3. Put `custom_menu.zip` contents at the root of the client addon (`panorama/layout/...`, `panorama/styles/...`)
4. `menu_buttons.zip` is only for the old WASD menu, unchanged from 1.1.2
