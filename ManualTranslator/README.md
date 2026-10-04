# Infinite Lives - Community Translation Mod 🌍

A community-driven localization mod for MDickie's **Infinite Lives**. This mod dynamically translates the game's menus, interface, and NPC dialogues in real-time using a simple `.txt` dictionary file. 

Built with the community in mind, this tool allows anyone to translate the game into their native language without needing to write a single line of code!

---

## ✨ Features

* **100% Data-Driven:** Zero hardcoded strings in the code. Every single translated line is loaded from a simple, editable `.txt` file.
* **Smart Case Preservation:** The mod intelligently adapts to the game's casing. If your file says `Exit=Sair`, the mod will automatically output `SAIR` when the game asks for `EXIT`, and `Sair` when the game asks for `Exit`.
* **Dynamic Dialogue Support:** Safely translates concatenated dialogues (e.g., *"What's your problem, Oliver Gold?"*) by translating the phrase and dynamically preserving the generated NPC names.
* **Plug & Play:** Just drop the `.dll` and your `.txt` file into the plugins folder and you are ready to play.

---

## 📥 Installation (For Players)

*Note: Infinite Lives comes with the BepInEx modding framework pre-installed, so you do not need to download it separately!*

1. Go to the [Releases](../../releases) page and download **`ModDialogos.dll`** along with a translation file for your language (e.g., `traducao_infinitelives.txt`).
2. Open your Infinite Lives game directory.
3. Navigate to `BepInEx/plugins/Manual`.
4. Drop both the `.dll` and the `.txt` file into that folder.
5. Launch the game and enjoy!

---

## ✍️ How to Translate (For Contributors)

Want to translate Infinite Lives into your own language? It's incredibly easy:

1. Grab the **`traducao_infinitelives_base.txt`** file from this repository. This is a clean template containing over 8,000 extracted UI and dialogue lines.
2. Open it in Notepad (or any text editor).
3. Translate the text after the `=` sign. 
   * **Format:** `Original English Text=Your Translated Text`
   * **Example:** `Play=Jugar`
4. Save the file as **`traducao_infinitelives.txt`** and put it in your game's plugin folder to test it.

### ⚠️ Important Translation Rules:
* **Rich Text Tags:** If the original text has Unity formatting tags (like `<color=red>` or `<b>`), you **must** keep them in your translation.
  * *Correct:* `<color=red>Blood</color>=<color=red>Sangre</color>`
  * *Wrong:* `<color=red>Blood</color>=Sangre`
* **Do not edit the left side:** The text on the left of the `=` sign is what the game's engine looks for. Never change it!

---

## 🛠️ Technical Info

This mod uses **BepInEx 5.x** and **HarmonyX** to patch Unity's UI components (`UnityEngine.UI.Text` and `TMPro.TextMeshProUGUI`). 
It does **not** require ILCCL (Infinite Lives Custom Content Loader), as ILCCL handles 3D models/audio and has no text-localization features.

---

## 📄 License & Disclaimer

This project is licensed under the [MIT License](LICENSE).

> **Disclaimer:** This is an unofficial, fan-made, non-profit community mod. "Infinite Lives" and all related original texts, lore, and assets are the property of **MDickie**. This project was created for the community to enable localization and accessibility, and is not affiliated with, endorsed by, or sponsored by MDickie.
