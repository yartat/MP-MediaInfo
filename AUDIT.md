# Аудит MP-MediaInfo: ошибки, уязвимости, некорректное поведение

Дата аудита: 2026-11-18. Обновлено: 2026-11-19. Ветка `feature/next-revision`, коммит `b059737aa`; исправления из этого отчёта лежат в рабочем дереве поверх него.

Охват: `MediaInfo.Wrapper` (включая устаревший `MediaInfoWrapper`), `MediaInfo.Analysis.Rtsp`, `MediaInfo.TestFilesGenerator`. `Samples` исключены по условию. Сама MediaInfoLib (C++) не аудировалась, но её исходники использовались, чтобы проверить, какие значения она отдаёт.

Метод: ручное чтение кода плюс три проверки инструментами:
- сверка порядка `NativeMethods.*` с CSV полей MediaInfoLib 26.10 и upstream master;
- `dotnet list package --vulnerable --include-transitive` для обоих решений;
- сверка с исходниками MediaInfoLib: как склеиваются значения HDR и проверяются ли handle.

Динамических проверок при самом аудите не было. Для исправленных находок позже добавлены тесты, в том числе с настоящим TCP-сервером для RTSP.

Предыдущий отчёт `SECURITY_AUDIT.md` (2026-11-17) этот документ заменяет. Находки оттуда, относящиеся не к `Samples`, перепроверены по текущему коду и помечены «(повтор)».

## Сводка

| № | Серьёзность | Тип | Находка | Статус |
|---|---|---|---|---|
| 1 | Высокая | Поведение | Dolby Vision и HDR10+ определяются как HDR10, если в потоке несколько HDR-форматов | Исправлено |
| 2 | Высокая | Поведение | Чтение полей по индексу ломается при другой версии нативной библиотеки; `ImageStreamBuilder` с upstream читает не те поля | Проверка добавлена |
| 3 | Высокая | Поведение / память | На macOS `wchar_t` считается 2-байтовым: строки искажаются, нативный код читает за границей буфера | Пропущено (нет macOS) |
| 4 | Высокая | Поведение / DoS | `MediaSource.From(каталог)` рекурсивно обходит всё дерево и анализирует первый найденный в глубине DVD/BD | Исправлено |
| 5 | Высокая | Уязвимость | RTSP: `Content-Length` от сервера без ограничения, выделение до 2 ГБ (повтор) | Исправлено |
| 6 | Средняя | Поведение | `TimeoutAnalyzer` не ограничивает время, пока нативный вызов не вернётся | Исправлено |
| 7 | Средняя | Поведение | `RetryingAnalyzer` повторяет анализ уже прочитанного или закрытого потока | Исправлено |
| 8 | Средняя | Поведение | RTSP: исключение из `TearDownAsync` превращает успешный захват в ошибку | Ложная находка |
| 9 | Средняя | Устойчивость | Огромная длительность в файле даёт `OverflowException` и проваливает весь анализ | Исправлено |
| 10 | Средняя | Поведение | Кэш отдаёт общий изменяемый результат; ключ может бросить исключение; `HasExternalSubtitles` устаревает | Исправлено частично |
| 11 | Средняя | Поведение | Обход папки: вложенные диски не распознаются; один недоступный подкаталог обнуляет весь результат | Исправлено |
| 12 | Средняя | Уязвимость | RTSP: неограниченный буфер фрагмента (повтор) | Исправлено |
| 13 | Средняя | Уязвимость | Учётные данные из URL попадают в `SourcePath`, сообщения, прогресс и логи (повтор, шире) | Открыто |
| 14 | Средняя | Уязвимость | RTSP: нет таймаутов на DESCRIBE/SETUP/PLAY, число заголовков не ограничено (повтор) | Исправлено |
| 15 | Средняя | Уязвимость | RTSP: Basic по открытому каналу, понижение схемы, Digest без `qop` (повтор) | Открыто |
| 16 | Средняя | Уязвимость | Загрузка нативных DLL по относительному пути, ошибки загрузки игнорируются (повтор) | Открыто |
| 17 | Средняя | Поведение | `setlocale` меняет глобальную локаль процесса из параллельных потоков | Исправлено |
| 18 | Низкая | Поведение | Зависимость от текущей культуры (`ToUpper`/`ToLower`) | Открыто |
| 19 | Низкая | Поведение | Fallback-читатель дисков не срабатывает на исключение; дубликат ключа на регистрозависимой ФС | Открыто |
| 20 | Низкая | Поведение | `WithRtsp` игнорирует `UseFileSystem` | Открыто |
| 21 | Низкая | Поведение | Мелкие ошибки моделей и билдеров (см. раздел) | Открыто |
| 22 | Низкая | Устойчивость | Мелкие проблемы декораторов и потокового анализа (см. раздел) | Открыто |
| 23 | Низкая | Уязвимость | IFO/MPLS без лимита размера, квадратичный MPLS, переполнение в IFO, wildcard в имени (повтор) | Открыто |
| 24 | Низкая | Устойчивость | Устаревший `MediaInfoWrapper`: рекурсия без защиты от циклов ссылок | Открыто |

Зависимости: известных уязвимостей в NuGet-пакетах нет (оба решения, включая транзитивные).

## Что сделано

| № | Что изменено | Тесты |
|---|---|---|
| 1 | `HdrUtils.TryGetHdr` разбирает список через ` / ` и выбирает самый специфичный формат | `LibraryCompatibilityTests`, `DolbyVisionTests` |
| 2 | `LibraryCompatibility` сверяет версию, `FieldLayoutInspector` сверяет имя каждого индекса с `NativeMethods`; расхождения идут в `Warnings` результата, устаревшего `MediaInfoWrapper` и в лог. Значения не исправляются | `LibraryCompatibilityTests`; интеграционный тест проверяет отсутствие предупреждений на настоящей 26.10 |
| 4 | `IsDvD` и `IsBluRay` без рекурсии, точное сравнение расширения | `DiscFolderDetectionTests` |
| 5 | `RtspConnection.ParseContentLength`, лимит тела 1 МиБ | `RtspHostileServerTests` |

| 12 | `VideoDepacketizer.MaximumFragmentSize`, освобождение буфера, конец отброшенного блока ничего не пишет | `RtspServerBehaviourTests` (раздел «Fragment limit») |
| 14 | Таймаут каждого чтения и бюджет на весь ответ, лимит числа и размера заголовков | `RtspServerBehaviourTests` |
| 6 | Вызывающий не ждёт нативный вызов после отмены или таймаута | `AnalyzerRobustnessTests` |
| 7 | Потоки не повторяются, задержка повтора ограничена минутой | `AnalyzerRobustnessTests` |
| 9 | `DurationConverter`, знак в `TryGetDouble` | `AnalyzerRobustnessTests` |
| 10 | Ключ кэша не бросает, субтитры вне кэша (порядок декораторов изменён) | `AnalyzerRobustnessTests`, `BuilderTests` |
| 11 | Обход папки по одной, диски на любой глубине, недоступные подпапки пропускаются | `AnalyzerRobustnessTests`, `FolderScanTests` |
| 17 | `NativeLocale`: `setlocale` один раз под блокировкой | `AnalyzerRobustnessTests` |

Для устаревшего `MediaInfoWrapper` пункт 4 меняет поведение: путь к папке с вложенными дисками больше не открывается, раньше он случайно находил первый диск.

Пункт 2 ничего не исправляет в значениях: пока нативная библиотека не совпадает с `NativeMethods`, `ImageStreamBuilder` и остальные билдеры читают по индексу чужие поля. Предупреждение лишь сообщает об этом. Перевод `ImageStreamBuilder` на чтение по имени отложен по решению владельца.

Состояние проверок после исправлений: Debug и Release собираются, `MediaInfo.Analysis.Tests` 215, `MediaInfo.Analysis.Rtsp.Tests` 88 (6 пропущены), интеграционные 64 (10 пропущены). В `MediaInfo.Wrapper.Core.Tests` падают 14 тестов, из них 7 (`Load3dDemo`, 6 `LoadUhdDemo`) падают и на чистом `HEAD`, остальные ждут `localhost`-сервер (`LoadAVSteam`) и файлы от `TestFilesGenerator`.

---

## Высокая

### 1. Dolby Vision и HDR10+ определяются как HDR10 — исправлено

Исправлено 2026-11-19: `HdrUtils.TryGetHdr` разбирает список через ` / ` и выбирает самый специфичный формат (Dolby Vision, HDR10+, HDR Vivid, SL-HDR, HLG+, HLG, HDR10). Тесты: `LibraryCompatibilityTests`, `DolbyVisionTests`.

`Builder/HdrUtils.cs:53`, `Builder/HdrInfoBuilder.cs` (`GetFormat`).

`HdrUtils.TryGetHdr` ищет значение в словаре целиком. MediaInfoLib, когда в потоке несколько HDR-форматов, склеивает их через ` / ` (`File__Analyze_Streams.cpp`, `DolbyVision_Streams_Fill`: `Formats.Separator_Set(0, " / ")`). Пример: Dolby Vision профиль 8.1 даёт `HDR_Format = "Dolby Vision / SMPTE ST 2086"` и `HDR_Format_Compatibility = "HDR10 / HDR10"`. Ни одна из строк в словаре не находится, и `GetFormat` доходит до `transfer_characteristics = "PQ"`, то есть до `Hdr.HDR10`.

Итог: самые распространённые файлы с Dolby Vision (профили 7 и 8.x) и HDR10+ (`SMPTE ST 2094 App 4 / SMPTE ST 2086`) получают `VideoStream.Hdr = HDR10`. Тестов с составными значениями нет. Логика не новая, она перенесена из `VideoStreamBuilder`.

Исправление: разбивать значение по ` / ` и выбирать формат по приоритету (Dolby Vision, HDR10+, HDR Vivid, SL-HDR, HLG, HDR10). Добавить тест на `"Dolby Vision / SMPTE ST 2086"`.

### 2. Чтение полей по индексу зависит от точной версии нативной библиотеки — добавлена проверка

Добавлено 2026-11-19: `LibraryCompatibility` (версия) и `FieldLayoutInspector` (имя каждого индекса против enum). Несовпадение даёт предупреждение в `MediaAnalysisResult.Warnings`, `MediaInfoWrapper.Warnings` и лог. Сами значения не исправляются: `ImageStreamBuilder` по-прежнему читает `Width`, `Height` и др. по индексу.

`NativeMethods.cs` (enum `General`, `Video`, `Audio`, `Text`, `Other`, `Image`, `Menu`), все билдеры, `VideoTagBuilder`, `AudioTagBuilder`.

Билдеры вызывают `MediaInfo_GetI` с номером поля `(int)NativeMethods.X.Y`. Номер верен, только если порядок полей в загруженной библиотеке совпадает с enum. Сверка скриптом:

| Набор | MediaInfoLib 26.10 | upstream master |
|---|---|---|
| General, Video, Audio, Text, Other, Menu | совпадает | совпадает |
| Image | совпадает | расходится с индекса 43 (`Format_Settings_SliceCount` и `Rotation` вставлены в середину) |

Что получит `ImageStreamBuilder` с upstream-библиотекой (а `CLAUDE.md` прямо требует её для `GainMap`, `Rotation` и `DolbyVision`):

| Свойство | Индекс | Фактически читается |
|---|---|---|
| `Width` | 56 | `Codec_Info` |
| `Height` | 62 | `Width_Original` |
| `ColorSpace` | 82 | `Active_DisplayAspectRatio` |
| `ChromaSubsampling` | 83 | `Active_DisplayAspectRatio/String` |
| `BitDepth` | 86 | `ColorSpace` |

Ошибки нет, значения просто молча неверные. Та же опасность для всех потоков при любом будущем обновлении библиотеки, где в CSV вставят поле. `VideoTagBuilder` и `AudioTagBuilder` к тому же кладут значения в словарь по индексу, и ключи там перепутаются.

Исправление (одно из):
- в `ImageStreamBuilder` (и в новом коде вообще) читать поля по имени, как уже сделано для `Rotation` и `SliceCount`;
- при создании handle сравнивать `Info_Version` с версией, из которой сгенерирован `NativeMethods`, и при расхождении переключаться на чтение по имени или писать предупреждение;
- добавить в CI проверку порядка enum против CSV, как в этом аудите.

### 3. macOS: `wchar_t` обрабатывается как 2-байтовый — пропущено

Не исправлено: нет macOS для проверки. Вернуться, когда будет доступ к реальному Mac.

`Marshalling/UnicodeWCharStringMarshaller.cs:37`, `WCharTStringMarshaller.cs:65,92`, `NativeOwnedWCharTStringMarshaller.cs:54,116`.

Все маршалеры выбирают UTF-16 для `Windows || OSX` и UTF-32 только для остальных. На macOS (Apple clang) `sizeof(wchar_t) == 4`, а C-API MediaInfoLib принимает и возвращает `wchar_t*`. Последствия:
- **Входные строки** (путь в `Open`, имя параметра в `Get`, `Option`): нативный код читает UTF-16-буфер четырёхбайтовыми символами. Имя файла искажается, открытие не удаётся. Терминатор занимает 2 байта, а ищется 4-байтовый ноль, поэтому возможно чтение за концом выделенной памяти.
- **Выходные строки**: UTF-32 читается как UTF-16, и `PtrToStringUni` останавливается на первом символе. Почти все строковые значения будут длиной в один символ.

Это противоречит записи в release notes 26.10 «Unicode file names on Linux and macOS». Похоже, проверялся только Linux. Нужна проверка на реальном Mac. Если поставляемая `libmediainfo.dylib` не собрана с `-fshort-wchar` (обычно не собирается), надо убрать `OSX` из условий во всех четырёх маршалерах.

Там же: `setlocale_LC_CTYPE = "C.UTF-8"` (`Analysis/Native/MediaInfoLibFactory.cs:32`). Есть ли локаль `C.UTF-8` на macOS, не проверено. Если её нет, преобразование пути через `wcstombs` снова работает в локали «C».

### 4. `MediaSource.From(каталог)` обходит всё дерево и выбирает случайный диск — исправлено

Исправлено 2026-11-19: `IsDvD` и `IsBluRay` смотрят только саму папку и её `VIDEO_TS`/`BDMV`, без рекурсии, и сравнивают расширение точно (`.IFOX` больше не считается `.IFO`). Папка, в которой лежат диски, диском не считается. Для устаревшего `MediaInfoWrapper` это значит, что путь к папке с вложенными дисками больше не открывается. Тесты: `DiscFolderDetectionTests`.

`Extensions/FileNameExtensions.cs:431` (`IsComplexStructure`), вызывается из `MediaSource.From` для `IsDvD`, затем `IsBluRay`.

Для каталога выполняется `Directory.GetFiles(path, "*.IFO", рекурсивно)`, а если ничего не нашлось, то `*.BDMV` по всему поддереву, и берётся `files.First()`. Последствия:
- `AnalyzeAsync("D:\Movies")` синхронно обходит всю медиатеку дважды, до того как асинхронный анализ вообще начнётся. Отмены нет. Для `"C:\"` или `"/"` это обход всего диска.
- Если в поддереве есть хоть один DVD или BD, результатом для `D:\Movies` станет анализ этого произвольного (первого найденного) диска, а не ошибка «это не диск».
- На .NET Framework `SearchOption.AllDirectories` бросает `UnauthorizedAccessException` на первом недоступном подкаталоге, и исключение уходит из `MediaSource.From` наружу.

Исправление: проверять только сам каталог и его прямые `VIDEO_TS`/`BDMV`, как это уже делают `DvdStructureReader.CanRead` и `BluRayStructureReader.CanRead`, без рекурсии.

### 5. RTSP: неограниченный `Content-Length` (повтор) — исправлено

Исправлено 2026-11-19: `RtspConnection.ParseContentLength` ограничивает тело 1 МиБ (`MaximumBodySize`), больше — `RtspProtocolException`. Тесты: `RtspHostileServerTests` (в том числе сервер, который объявляет 2 ГБ).

`MediaInfo.Analysis.Rtsp/Rtsp/RtspConnection.cs:296`. Не исправлено, подробности в прошлом отчёте. Сервер присылает `Content-Length: 2147483647`, и клиент выделяет массив до 2 ГБ, не прочитав ни байта. Исправление: лимит 64 КБ–1 МБ и `RtspProtocolException` при превышении.

---

## Средняя

### 6. `TimeoutAnalyzer` не ограничивает время — исправлено

Исправлено 2026-11-19: `MediaProbe.Run` не ждёт нативный вызов, когда сработал токен (`Task.WhenAny` с токеном). Вызывающий получает `Timeout` или `OperationCanceledException` сразу, а вызов доживает на своём потоке и сам отдаёт handle. Это не прерывает нативный вызов: файл или соединение остаются занятыми, пока он не вернётся. С `OffloadBlockingCalls = false` вызов идёт на потоке вызывающего, и освободить его по-прежнему нечем; это описано в документации опции. Тесты: `AnalyzerRobustnessTests` (раздел Timeout).

`Analysis/Decorators/TimeoutAnalyzer.cs`, `Analysis/Pipeline/MediaProbe.cs:95`.

`MediaProbe.Run` выполняет `Task.Run(action, token)`. Токен лишь не даёт задаче стартовать, а уже идущий `MediaInfo_Open` (зависший сетевой ресурс, медленный HTTP через libcurl) отменить нельзя. `TimeoutAnalyzer` ждёт `Inner.AnalyzeAsync` и получает управление, только когда нативный вызов сам вернётся. Обещанное «не дольше N» не выполняется. С `OffloadBlockingCalls = false` анализ вообще синхронный.

Исправление: в `MediaProbe.Run` использовать `task.WaitAsync(cancellationToken)` (.NET 6+; для netstandard2.1 через `Task.WhenAny`), чтобы вызывающий код освобождался по таймауту. Нативный вызов при этом доработает в фоне и освободит свой handle сам. Это нужно описать в документации.

### 7. Повтор анализа потока — исправлено

Исправлено 2026-11-19: `RetryingAnalyzer` не повторяет `StreamMediaSource`. Заодно задержка между попытками растёт не бесконечно, а до одной минуты (`NextDelay`), так что длинная серия попыток не переполняет `TimeSpan`. Тесты: `AnalyzerRobustnessTests` (раздел Retry).

`Analysis/Decorators/RetryingAnalyzer.cs`, `Strategies/StreamAnalysisStrategy.cs`.

Повтор срабатывает на `NativeOpenFailed` и `Unknown`, и `Unknown` бывает у любого исключения при анализе потока. Второй проход:
- при `leaveOpen: true` читает поток с текущей позиции (с конца). Анализ пустой, возвращается `NoStreamsFound` вместо исходной ошибки;
- при `leaveOpen: false` поток уже закрыт, возвращается `StreamNotReadable`.

В обоих случаях настоящая причина теряется. Исправление: не повторять для `StreamMediaSource`, либо для seekable-потока возвращать позицию в начало.

### 8. RTSP: ошибка при TEARDOWN проваливает успешный захват — ложная находка

Проверено 2026-11-19, находка была неверной. `RtspProtocolException` наследует `IOException` (`RtspConnection.cs`, конец файла), а `TearDownAsync` ловит `IOException`, поэтому исключение «The server closed the connection while answering TEARDOWN» перехватывается и захват сохраняется. Это подтверждает тест `RtspServerBehaviourTests.Capture_ServerLeavesAfterTheMedia_KeepsWhatWasCaptured`: сервер отдаёт три кадра и закрывает соединение, захват возвращается с данными. Код `TearDownAsync` не менялся. Текст ниже оставлен как был, чтобы было видно, что именно проверялось.

`Rtsp/RtspStreamCapture.cs:133` (`finally { await TearDownAsync(...) }`), `TearDownAsync`.

`TearDownAsync` ловит только `IOException`, `OperationCanceledException` и `SocketException`. Но `SendAsync` бросает `RtspProtocolException("The server closed the connection while answering TEARDOWN.")`, когда сервер закрывает соединение. Это обычная ситуация: камера рвёт поток или соединение уже разорвалось, и цикл захвата завершился через `ReadBlockAsync == null`. Исключение вылетает из `finally`, и уже собранные кадры отбрасываются: стратегия возвращает «Could not capture».

Исправление: в `TearDownAsync` ловить и `RtspProtocolException`, а лучше любое исключение, кроме отмены вызывающим.

### 9. Огромная длительность ломает весь анализ — исправлено

Исправлено 2026-11-19: новый `DurationConverter` возвращает `TimeSpan.Zero` вместо `OverflowException` для значений, которые `TimeSpan` не вмещает; он используется в видео-, аудио- и субтитровых билдерах. `TryGetDouble` теперь принимает знак, поэтому `Rotation = -90` больше не превращается в 0. В устаревшем `MediaInfoWrapper` приведение `(int)duration` для огромных значений не менялось. Тесты: `AnalyzerRobustnessTests` (раздел Durations).

`Builder/VideoStreamBuilder.cs:397`, `AudioStreamBuilder.cs:287`, `SubtitleStreamBuilder.cs:76`.

`TimeSpan.FromMilliseconds(...)` вызывается без проверки диапазона. Испорченный или специально созданный файл (например, MP4 с 64-битной длительностью при timescale 1) даёт значение больше `TimeSpan.MaxValue` (около 9,2·10¹⁴ мс). Возникает `OverflowException`, и анализ всего файла возвращает `Unknown` вместо частичного результата. `DurationReader` такую проверку уже делает (`IsUsable`), билдеры нет.

Там же: `TryGetDouble` разбирает с `NumberStyles.AllowDecimalPoint`, поэтому отрицательные значения (`Rotation = -90`) не разбираются и превращаются в 0.

### 10. Кэш — исправлено частично

Исправлено 2026-11-19:
- `CachingAnalyzer` не бросает исключение, если файловая система не принимает путь (`ArgumentException`, `NotSupportedException`, `IOException`, `UnauthorizedAccessException`): такой анализ просто идёт без кэша. `MediaInfoFileSystem.GetFileLength` и `GetLastWriteTimeUtc` тоже перестали бросать на таких путях.
- `ExternalSubtitleAnalyzer` перенесён снаружи кэша, поэтому субтитры, появившиеся позже, замечаются. Это меняет порядок декораторов (сверху вниз: Logging, Validating, ExternalSubtitle, Caching, Timeout, Retrying, Throttling): обновлены `MediaInfoAnalyzerBuilder`, `docs/async-api.md` и тест `Build_ComposesTheDocumentedOrderWhicheverOrderTheConcernsAreRequestedIn`.

Не исправлено по решению: кэш по-прежнему отдаёт один и тот же объект результата, а потоки (`VideoStream` и др.) изменяемые. Копирование было бы неполным (списки, теги), поэтому результат из кэша нужно считать доступным только для чтения. Одновременные промахи по одному ключу всё ещё запускают несколько анализов.

Тесты: `AnalyzerRobustnessTests` (раздел Cache).

`Analysis/Decorators/CachingAnalyzer.cs`, `IAnalysisCache.cs`, `Analysis/Abstractions/MediaInfoFileSystem.cs:77`.

- Из кэша отдаётся тот же объект `MediaAnalysisResult`. Он `record`, но внутри изменяемые `VideoStream`, `AudioStream` и т.д. с публичными сеттерами. Если один вызывающий их изменит, изменится кэш для всех. `LegacyResultAdapter` копирует списки, но не объекты.
- `CreateKey` вызывается до вложенного анализатора и не защищён. `new FileInfo(path)` бросает `ArgumentException` на пути с `\0` (на .NET Framework ещё и на недопустимых символах, `NotSupportedException` на `:`), а `GetFileLength` ловит только `IOException` и `UnauthorizedAccessException`. Исключение уходит вызывающему вместо результата с ошибкой, в обход договора «ошибка — это `Failure`».
- `ExternalSubtitleAnalyzer` стоит внутри кэша, а ключ учитывает только сам видеофайл. Добавленный позже файл субтитров не меняет `HasExternalSubtitles`, пока не истечёт срок записи.
- Нет защиты от одновременного промаха: N параллельных запросов одного файла дают N нативных анализов.

### 11. Обход папки (`AnalyzeFolderAsync`) — исправлено

Исправлено 2026-11-19: обход идёт по одной папке за раз. Диск находится на любой глубине и отдаётся одним результатом, внутрь диска обход не заходит, поэтому служебные файлы `IFO`, `BDMV`, `MPLS` больше не попадают в анализ как отдельные медиа. Глубина ограничена 64 уровнями (петли ссылок). Недоступная папка теряет только себя. `MediaInfoFileSystem.GetFiles` пропускает недоступные подпапки вместо того, чтобы вернуть пустой массив, поэтому `TotalSize` дисков Blu-ray тоже не обнуляется. Тесты: `AnalyzerRobustnessTests` (раздел Folder scan) и существующие `FolderScanTests`.

`Analysis/MediaFolderScan.cs:121`, `Analysis/Abstractions/MediaInfoFileSystem.cs`.

- Диски ищутся только среди прямых подкаталогов. В рекурсивном режиме диск глубже (`Movies/Collection/Film/VIDEO_TS`) не распознаётся, и каждый VOB или M2TS анализируется как отдельный файл. Кроме того, `IsVideo()` включает расширения `.IFO`, `.BDMV`, `.MPLS`, так что служебные файлы дисков тоже попадают в анализ.
- `Directory.GetFiles(..., SearchOption.AllDirectories)` на .NET использует `IgnoreInaccessible = false`. Один недоступный подкаталог бросает исключение, `MediaInfoFileSystem.GetFiles` его ловит и возвращает пустой массив. Весь обход молча выдаёт ноль результатов. Та же причина обнуляет `TotalSize` Blu-ray в `MplsBluRayStructureReader` и `BluRayStructureReader`. Исправление: `EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = ... }`.

### 12 и 14. RTSP: буфер фрагмента, таймауты и заголовки — исправлено

Исправлено 2026-11-19.
- **12.** `VideoDepacketizer.MaximumFragmentSize` (по умолчанию 8 МиБ, `RtspStreamCapture` ставит `MaximumCaptureBytes`). Фрагментированный блок, который вырос больше лимита, отбрасывается (`UnitsDropped`), буфер освобождается, а конец отброшенного блока ничего не пишет.
- **14.** `FillAsync` ограничивает каждое чтение `ReceiveTimeout`, а `SendOnceAsync` ограничивает весь ответ четырьмя `ReceiveTimeout`, поэтому сервер, который отвечает по байту, тоже не держит вызов бесконечно. Таймаут при ожидании ответа даёт `RtspProtocolException` («did not answer DESCRIBE», «did not finish answering»), а во время захвата заканчивает захват, как закрытое соединение, не теряя собранное. Ответ ограничен 128 заголовками и 64 КиБ. Строка, оборванная таймаутом, не считается законченной.
- Тесты: `RtspServerBehaviourTests` (молчащий сервер, ответ по байту, 500 заголовков, обрыв после кадров, тишина после первого кадра, лимит фрагмента).

### 13, 15, 16. RTSP и загрузка DLL (повторы)

Не исправлены, подробности в `SECURITY_AUDIT.md`:
- **13** — учётные данные из URL. Помимо RTSP, это касается любого `NetworkMediaSource`: `DisplayName`, `SourcePath`, `AnalyzedPath`, сообщения (`NativeOpenFailed ... could not open '{path}'`), прогресс, `LoggingAnalyzer`. Для `http(s)://user:pass@` то же самое.
- **15** — `RtspAuthenticator`: Basic принимается всегда, даже после Digest. Нет `rtsps`. Digest без `qop` и `cnonce`, параметры не экранируются.
- **16** — `MediaInfo.cs`, `MediaInfoWrapper.GetPathToMediaInfo`: на .NET Framework первым проверяется `"./"` (рабочий каталог). Результаты `LoadLibraryEx` и `NativeLibrary.TryLoad` для зависимостей игнорируются. Нет `DefaultDllImportSearchPaths`.

### 17. `setlocale` из параллельных потоков — исправлено

Исправлено 2026-11-19: `NativeLocale.Shared.Ensure` вызывает `setlocale_LC_CTYPE` один раз на процесс под блокировкой, и до этого ни один handle не открывает файл. Используется и в `MediaInfoLibFactory`, и в устаревшем `MediaInfoWrapper`. На Windows ничего не делает. Тесты: `AnalyzerRobustnessTests` (раздел Locale, 32 параллельных handle дают один вызов).

`Analysis/Native/MediaInfoLibFactory.cs:32`.

На Linux и macOS каждый новый handle вызывает `Option("setlocale_LC_CTYPE", "C.UTF-8")`, то есть `setlocale()` процесса. Это глобальное состояние, и `setlocale` не потокобезопасен. При `WithConcurrencyLimit(n > 1)` или параллельном `AnalyzeFolderAsync` вызовы идут одновременно с работой других потоков, в том числе с `wcstombs` внутри MediaInfoLib. Это гонка данных (UB в glibc). Заодно меняется локаль хост-приложения. Исправление: выполнять один раз под блокировкой (`Lazy`/static), документировать побочный эффект.

---

## Низкая

### 18. Зависимость от культуры

- `SubtitleStreamBuilder.TryGetCodec`: `source.ToUpper()` с текущей культурой. В турецкой и азербайджанской культуре `i` превращается в `İ`, и коды вроде `dvb_subtitle` не находятся.
- `LanguageMediaStreamBuilder`: `Get("Language").ToLower()`; `AudioStreamBuilder`: `CodecName = ...ToUpper()`; `VideoStreamBuilder.GetFullCodecName`: `ToUpper()`. Нужно использовать `ToUpperInvariant` и `ToLowerInvariant`.
- `TagBuilderHelper.TryGetInt` и `TryGetLong` разбирают с текущей культурой. Для целых это почти всегда безопасно, но стоит явно указать `CultureInfo.InvariantCulture`.

### 19. Чтение дисков

- `FallbackDiscStructureReader.ReadAsync`: запасной читатель вызывается только при `null`, а не при исключении. Любая ошибка предпочтительного читателя проваливает анализ, хотя папочный читатель справился бы.
- `MplsBluRayStructureReader.cs:87` и `DvdStructureReader.cs:112`: `ToDictionary(..., OrdinalIgnoreCase)`. На регистрозависимой ФС `00001.m2ts` и `00001.M2TS` дают `ArgumentException` (с учётом предыдущего пункта весь анализ падает).

### 20. `WithRtsp` игнорирует `UseFileSystem`

`MediaInfo.Analysis.Rtsp/RtspAnalyzerExtensions.cs:44`: стратегии по умолчанию создаются с `MediaInfoFileSystem.Instance`. Дисковые стратегии читают реальную ФС, остальные — подставленную.

### 21. Модели и билдеры

- `ChapterStreamBuilder.Kind` возвращает `MediaStreamKind.Menu` для потоков `Other`.
- `AudioStreamBuilder.cs:319` создаёт `AudioTagBuilder(Info, StreamPosition)`, а базовый `GeneralTagBuilder` читает General-поток с позицией аудиопотока. У аудиопотока 0 теги файла дублируются (обложки декодируются из base64 повторно), у потоков 1 и далее они пустые. Тот же приём в `VideoTagBuilder`.
- `VideoTagBuilder` и `AudioTagBuilder` опрашивают все поля enum (около 400 и 285 вызовов P/Invoke с выделением строк) на каждый поток. Заметно на больших пакетах.
- `RtspAnalysisStrategy.cs:203`: `PCMA` и `PCMU` дают `AudioCodec.Undefined`, хотя есть `PcmALaw` и `PcmMuLaw` (комментарий устарел).
- `H265Depacketizer` не обрабатывает поля DONL/DOND (`sprop-max-don-diff > 0`), такие потоки собираются неверно.
- `FileNameExtensions.IsRtsp` ищет `rtsp:` в любом месте строки: `http://host/?u=rtsp:...` и путь Linux `/mnt/rtsp:cam.mkv` уходят в RTSP-стратегию.
- `VideoStreamBuilder.GetFullCodecName` вычисляет `strCodec`, но не использует его (мёртвый код).

### 22. Декораторы и потоковый анализ

- `RetryingAnalyzer.cs:81`: `delay += delay` без верхней границы. Примерно после 40 попыток `TimeSpan` переполняется (`OverflowException`).
- `ThrottlingAnalyzer`: `Dispose` во время выполняющихся анализов освобождает семафор, и их `Release()` бросает `ObjectDisposedException`.
- `StreamAnalysisStrategy`: `BufferSize` не проверяется (0 даёт молча пустой результат, отрицательный — исключение). Для не-seekable потока нет лимита на объём: бесконечный поток читается, пока не отменят.
- `MediaInfo.Close()` вызывает `MediaInfo_Delete`, а не `MediaInfo_Close`, то есть после `Close` handle непригоден. Имя вводит в заблуждение, в том числе через `INativeMediaInfo.Close`.

### 23. Парсеры дисков (повторы)

IFO и MPLS читаются в память без лимита размера (`IfoDvdStructureReader.Read`, `MplsBluRayStructureReader.Read`). Разбор MPLS квадратичный. Есть переполнение `U32 * SectorSize` в `IfoParser`. Шаблон `Directory.GetFiles` содержит пользовательское имя файла, wildcard-символы не экранированы. Подробности в `SECURITY_AUDIT.md`, пункты 10–12 и 14.

### 24. Устаревший `MediaInfoWrapper`

- `MediaInfoWrapper.cs:480` (`GetDirectorySize`): рекурсия по `GetDirectories` идёт через символические ссылки и junction. Цикл ссылок внутри папки Blu-ray или DVD даёт `StackOverflowException`, который нельзя перехватить: процесс падает. Исключения доступа тоже не ловятся.
- Конструктор от `Stream` вызывает `inputStream.Length` до `try`. Не-seekable поток бросает `NotSupportedException` из конструктора, а не даёт `Success = false`.

---

## Проверено и признано корректным

- `BigEndian`, `IfoParser`, `MplsParser`: все чтения с проверкой границ (подтверждено повторно).
- `RtpPacket.TryParse`, `H264Depacketizer`, `H265Depacketizer.WriteAggregated`: длины проверяются до `Slice`.
- Интерливинг RTSP: длина блока 1–65535, буфер фиксированный.
- Маршалинг на Windows и Linux: выделение и освобождение симметричны.
- Вызов MediaInfoLib с нулевым или освобождённым handle безопасен: DLL проверяет handle (`MediaInfoDLL.cpp`, `MI_Outputs.find(Handle)`, `Handle == NULL`), поэтому использование `MediaInfoList` после `Dispose` не роняет процесс.
- `ValidatingAnalyzer`, `LoggingAnalyzer`, `PriorityStrategySelector`, `MemoryAnalysisCache` (кроме пункта 10): ошибок не найдено.
- `TestFilesGenerator`: FFmpeg вызывается в процессе через библиотеки, без командной оболочки. Удаляются только собственные выходные файлы.
- Порядок enum `NativeMethods` полностью совпадает с MediaInfoLib 26.10 для всех типов потоков.
- NuGet: известных уязвимостей нет.

## Порядок исправления

Сделано: 1, 2, 4, 5, 6, 7, 9, 10 (частично), 11, 12, 14, 17. Ложная находка: 8. Пропущено: 3 (нужен Mac).

Дальше по приоритету:
1. **Пункты 13, 15, 16** — учётные данные в URL, схемы аутентификации RTSP, безопасная загрузка нативных DLL.
2. Остальное (18–24).
3. Перевод `ImageStreamBuilder` на чтение по имени, если нужен корректный результат с нативной библиотекой новее 26.10 до её официального выпуска.

Для пунктов 13 и 15 есть готовая основа с настоящим TCP-сервером (`RtspHostileServerTests`, `RtspServerBehaviourTests`); пункты 13 и 15 можно проверять так же.
