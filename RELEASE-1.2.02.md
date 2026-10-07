## English

What is new in 1.2.02
- Built for CounterStrikeSharp 1.0.376.
- `MenuManagerCore.json` has `HudOpenDelay`, default `0.2`. It delays the normal panorama and CSGO Menu so a chat command and the HUD are not the same frame. The WeaponPaints locker does not use this pause: it has its own `OpenDelay`.
- `GetSelectedMenu` returns the player's real menu type, including WASD. `GetMenuType` still reports WASD as mouse panorama, so older VIP and IksAdmin checks keep working.
- SwiftlyS2 build: `MenuManagerCS2-1.2.02-swiftlys2.zip` (SwiftlyS2 1.4.12+). Same `HudOpenDelay` and `GetSelectedMenu`.

You can read more on the site: https://genesis-cs.space/menuconstructor/index.html

## Русский

Что нового в 1.2.02
- Собрано под CounterStrikeSharp 1.0.376.
- В `MenuManagerCore.json` поле `HudOpenDelay`, по умолчанию `0.2`. Пауза перед обычной панорамой и CSGO Menu, чтобы команда в чате и HUD не попадали в один кадр. Локер WeaponPaints этой паузой не управляется: у него свой `OpenDelay`.
- `GetSelectedMenu` отдаёт реальный тип меню, включая WASD. `GetMenuType` по-прежнему считает WASD панорамой мышью, чтобы старые проверки VIP и IksAdmin не отвалились.
- Сборка SwiftlyS2: `MenuManagerCS2-1.2.02-swiftlys2.zip` (SwiftlyS2 1.4.12+). Те же `HudOpenDelay` и `GetSelectedMenu`.

Вы можете ознакомиться с более подробной информацией на сайте: https://genesis-cs.space/menuconstructor/index.html
