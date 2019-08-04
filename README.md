# UnitySettingSwitcher

構造化された設定ファイルをもとにUnity設定を切り替えられるようにします。
設定ファイルでは名前付きで設定(Setting)を定義します。
SettingではUnityの各種設定(BuildSettings, PlayerSettings, XRSettings)にセットする値を定義します。
Settingは他のSettingの設定を継承(inherit)して差分設定を作ることができます。
Setting名は"/"で区切ることでグループ化することができます。（現在のところグループは１階層のみです）

## 利用例

- PC向けとVR向け、develop向けとproduction向けを切り替える。

## 使いかた

- 手順
  - UnitySettingSwitcherウィンドウを開く。(Window->Unity Setting Switcherを選択か、CMD+E)
  - "Create settings.json from template"ボタンをクリックすると、/Assets/ディレクトリにsettings.jsonが生成される。
  - settings.jsonを編集して設定を定義する。

## ファイル構成

### ディレクトリ構成

/<Assets>
	settings.json .. 設定定義ファイル
	settingsselected.local.json .. 選択されている設定を保存しているファイル

### settings.json JSON構造

- settings : array
  - name : string .. 設定名
  - scene_list : array<string>
    - シーンファイル(.scene)パス
  - player_settings : object
    - keyValues : array
      - key : 設定キー名
      - s : 設定値（対象の設定がstring型用）
      - b : 設定値（対象の設定がbool型用）
      - i : 設定値（対象の設定がint型用）
      - f : 設定値（対象の設定がfloat型用）
  - xr_settings : object
    - keyValues : ※player_settings同様
- 設定名
  - "."で始まっているとUnitySettingSwitcherウィンドウの設定一覧上に非表示になる。
- player_settings
  - UnityのPlayerSettingsクラスに対しての設定値
- xr_settings
  - UnityのXRSettingsクラスに対しての設定値

## 補足

- 設定の結合ルール
  - 設定は、グループごとの選択中な設定を結合したもの
- 設定の結合順序
  - 選択されている設定の定義されている順
  - 継承構造の子側から親側に順次
- JSONで記述された設定はC#のリフレクションを用いてBuildSettingsクラス等のpublic staticメンバーへ反映している。

## 現在の制約

* UnityPackageManagerには未対応
* 諸事情によりSettings.jsonのフォーマットが若干煩雑になっている
  (player_settings/配下にkeyValues項があるが、player_settings直下にjsonのkey-valueを入れたほうが記述しやすい・シンプルになる→UnityのJsonUtilityで実現する方法がわからなかった)