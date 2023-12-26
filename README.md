# Примеры перекрытий прикладной разработки
Репозиторий с примерами перекрытий прикладной разработки. 
Подробное описание и реализацию см. в ![Описание шаблона разработки перекрытий](docs/ОГВ. Описание шаблона разработки перекрытий.docx).

## Порядок установки
Для работы требуется установленный Directum RX версии 4.7 и выше.

### Установка для ознакомления
Склонировать репозиторий rx-govexamples в папку.
Указать в _ConfigSettings.xml DDS:
```
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" />
  <repository folderName="RX" solutionType="Base" url="<адрес локального репозитория>" />
  <repository folderName="<Папка из п.1>" solutionType="Work" 
     url="https://github.com/DirectumCompany/rx-govexamples" />
</block>
```
### Установка для использования на проекте
Возможные варианты:

#### A. Fork репозитория

Сделать fork репозитория rx-govexamples для своей учетной записи.
Склонировать созданный в п. 1 репозиторий в папку.
Указать в _ConfigSettings.xml DDS:
```
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" /> 
  <repository folderName="<Папка из п.2>" solutionType="Work" 
     url="<Адрес репозитория gitHub учетной записи пользователя из п. 1>" />
</block>
```
#### B. Подключение на базовый слой.

Вариант не рекомендуется, так как при выходе версии шаблона разработки не гарантируется обратная совместимость.

Склонировать репозиторий rx-govexamples в папку.
Указать в _ConfigSettings.xml DDS:
```
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" /> 
  <repository folderName="<Папка из п.1>" solutionType="Base" 
     url="<Адрес репозитория gitHub>" />
  <repository folderName="<Папка для рабочего слоя>" solutionType="Work" 
     url="https://github.com/DirectumCompany/rx-govexamples" />
</block>
```
#### C. Копирование репозитория в систему контроля версий.

Рекомендуемый вариант для проектов внедрения.

В системе контроля версий с поддержкой git создать новый репозиторий.
Склонировать репозиторий rx-govexamples в папку с ключом 
```
--mirror.
```
Перейти в папку из п. 2.
Импортировать клонированный репозиторий в систему контроля версий командой:
```
git push –mirror <Адрес репозитория из п. 1>
```
