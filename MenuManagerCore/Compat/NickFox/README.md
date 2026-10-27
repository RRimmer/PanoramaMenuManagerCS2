# Заглушки PlayerSettings / PlayerSettingsApi / AnyBaseLib

CS2-SimpleAdmin (и другие плагины, сделанные под MenuManagerCS2 от NickFox) перед стартом проверяют,
что на сервере лежат файлы:

- `plugins/PlayerSettings/PlayerSettings.dll`
- `shared/PlayerSettingsApi/PlayerSettingsApi.dll`
- `shared/AnyBaseLib/AnyBaseLib.dll`

PMM эти библиотеки не использует: настройки меню ядро хранит само. Здесь три пустые сборки с теми же именами,
только чтобы проверка прошла. В них нет `ISettingsApi` и нет базы.

Если какой-то плагин реально вызывает `ISettingsApi` (capability `settings:nfcore`), поставь вместо заглушек
оригинальные PlayerSettingsCS2 и AnyBaseLibCS2 от NickFox. Ядру PMM они не мешают.

Собираются отдельно от ядра (`MenuManagerCore.csproj` исключает `Compat\**`), см. `build.cmd`.
