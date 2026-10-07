## English

What is new in 1.2.00
- CSGO Menu. Orange panel: items 1–7, 8 back, 9 next, 0 exit. Commands css_1–css_0 and menuselect. In chat the fallback is !1–!0. While dead or spectating the game takes the number keys, so use chat. Before you enable it, the menu writes one bind line to chat and to the client console. In !menu, Configure CSGO Menu has position, binds to chat, and a death message you can turn off. If there is nowhere to go back, 8 is gray and does not close the menu.
- Panorama WASD. In !menu it sits next to mouse panorama and is saved in menumanager.db. W/S move the cursor, D selects, A goes back. AddSelect opens a submenu. On mouse, a list of up to 12 values still drops down on the row. On panorama mouse and WASD the words [On]/[Off] are hidden. Chat, center and CSGO Menu still show them.
- Panel position is left, center or right, shared by panorama and CSGO Menu. The player changes it in !menu. The admin default is PanoramaPosition. No new Workshop upload for a position change.
- Menu colors are chosen in the constructor and installed in the client addon. There is no theme field in the config.
- !menu order: Panorama WASD, Panorama mouse, CSGO Menu, Center (Chat), Center (WASD), Chat menu. Console is not in the list. The volume slider is gone.
- Chat and center buttons say Back, Next and Close.
- After replacing the DLL, restart the server. css_plugins reload does not unload the old assembly. After a panorama change the player has to rejoin.

Constructor, API and changelog: https://genesis-cs.space/menuconstructor/index.html

Supported plugins: https://genesis-cs.space/menuconstructor/menumanager/plugins/index.html

## Русский

Что нового в 1.2.00
- CSGO Menu. Оранжевая панель: пункты 1–7, 8 назад, 9 далее, 0 выход. Команды css_1–css_0 и menuselect. В чате запасные !1–!0. Мёртвым и в наблюдении цифры забирает игра, пиши в чат. Перед включением меню пишет одну строку бинда в чат и в консоль клиента. В !menu пункт «Настроить CSGO Menu»: позиция, бинды в чат и сообщение при смерти, его можно выключить. Если возвращаться некуда, 8 серая и меню не закрывает.
- Панорама WASD. В !menu рядом с панорамой мышью, выбор пишется в menumanager.db. W/S двигают курсор, D выбирает, A назад. AddSelect открывает подменю. На мыши список до 12 значений по-прежнему выпадает на строке. На панораме мышью и на WASD слова [Вкл]/[Выкл] скрыты. В чате, центре и CSGO Menu они остаются.
- Позиция панели: слева, центр или справа. Общая для панорамы и CSGO Menu. Игрок меняет её в !menu. Админский дефолт — PanoramaPosition. Для смены места новая мастерская не нужна.
- Цвет меню выбирается в конструкторе и ставится в клиентский аддон. В конфиге поля темы нет.
- Порядок в !menu: Панорама WASD, Панорама мышь, CSGO Меню, Центр (Чат), Центр (WASD), Чат меню. Консольного в списке нет. Ползунок громкости убран.
- Кнопки чата и центра подписаны: Назад, Далее, Закрыть.
- После замены DLL нужен полный перезапуск сервера. css_plugins reload старую сборку не выгружает. После замены панорамы игрок заходит на сервер заново.

Конструктор, API и список изменений: https://genesis-cs.space/menuconstructor/index.html

Поддерживаемые плагины: https://genesis-cs.space/menuconstructor/menumanager/plugins/index.html

**Полный изменения**: https://github.com/RRimmer/PanoramaMenuManagerCS2/compare/MenuManagerCS2-1.1.11...MenuManagerCS2-1.2.00
