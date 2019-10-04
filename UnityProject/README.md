# UnitySettingSwitcher

構造化された設定ファイルをもとにUnity設定を切り替えられるようにします。

![Screen Shot 2019-08-04 at 13 31 43](https://user-images.githubusercontent.com/4578728/62419488-4177fc00-b6bc-11e9-8409-19a2c3d287b3.png)

* 設定ファイルでは名前付きで設定(Setting)を定義します。
* SettingではUnityの各種設定(BuildSettings, PlayerSettings, XRSettings)にセットする値を定義します。
* 可能なこと
    * 他Settingを継承したSettingの定義
    * Settingのグループ分け
    * SettingSwitcherウィンドウからのSetting切替
    * Settingの有効・無効の切替
    * SwttingSwitcherウィンドウのSettingリストへ非表示なSettingの定義

* ※Settingは他のSettingの設定を継承(inherit)して差分設定を作ることができます。
* ※Setting名は"/"で区切ることでグループ化することができます。（現在のところグループは１階層のみ）
* ※各グループ内で選択してアクティブにできる設定は１つのみ

## 利用例

- PC向けとVR向け、develop向けとproduction向けを切り替える。

例)
![unity-settingswitcher-sample](https://user-images.githubusercontent.com/4578728/62419417-8438d480-b6ba-11e9-8c12-69abba41261e.png)
* 設定例
    * PC用設定を継承したWindows64向け設定とDevelop向け設定をマージしたものをUnityへ反映する
    * VR用設定を継承したSteamVR向け設定とProduction向け設定をマージしたものをUnityへ反映する


## 使いかた

- 導入
  - MiniJSONが必要です (https://gist.github.com/darktable/1411710)

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
    - 設定キー名(UnityのBuildSetting等該当クラスのpublic-staticメンバ名) : 値(string,bool,int,floatのみ対応)
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
