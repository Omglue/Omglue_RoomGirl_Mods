# 📌Omglue's RoomGirl Mods (July 2025)
This repository contains all of my mods for RoomGirl. You can message me in discord for anything really. Join the [Illusion discord server](https://discord.gg/illusionsoft) so discord doesn't block messages (Username: Omblue or Unique: lego_.__)

Here is a list of individual mod folders each with their own readme info text.
- _**If you are a user, these are not dowloads but informations for individual mods. Continue reading but if you are only looking to download go to [releases](https://github.com/Omglue/Omglue_RoomGirl_Mods/releases)**_
- [RoomGirl Extended Save](https://github.com/Omglue/Omglue_RoomGirl_Mods/tree/main/src/RG_ExtendedSave) - Needed for mods depending on it
- [RoomGirl Height Mod](https://github.com/Omglue/Omglue_RoomGirl_Mods/tree/main/src/RG_HeightMod) - Add a height slidder for male characters
- RoomGirl Api - In my wishlist of mod to make. Equivalent of the KKapi mod making modding much easier.
- RoomGirl Bone Mod - Add a menu for advanced body and bone customization like previous game's bone mod.
- And more i would like to make, or then it dies out and it's sad );

## About modding
Mods from previous illusion game's aren't compatible with this game even if the game code is almost the copy paste due to the choice to use the il2cpp compiler instead of mono compiler. It also made modding 10x more annoying to do since you can't see the code and can't use transpiler patchs. There is more info on the [wiki page](https://wiki.anime-sharing.com/hgames/index.php?title=Room_Girl/Modding).

Since Illusion [got out of buisnesse](https://x.com/ILLUSION_staff/status/1679660799185555456), modding for this game died because [modders felt](https://github.com/SpockBauru/SpockPlugins_Illusion/releases#Changelog) like there was no point anymore and so this game almost has no mods wich is kind of sad, so since i am a modder i'll try to do some. But i'll be modding other games too so yeah.

## 🧰Valuable infos for modding
_**‼️The rest of this readme text is intended for devs moddind this game, the informations and dowloads for users can be found in [releases](https://github.com/Omglue/Omglue_RoomGirl_Mods/releases)**_.

- Don't forget since this game is compiled with il2cpp, harmony _**transpiler**_ patchs can't work.

- Use dnSpy and open the assembly-csharp.dll of Honey Select 2 to look at the decompiled code because it's the closest we have to the code of Room Girl and that isn't compiled with il2cpp.

- Also don't forget to add the [Unity Explorer mod](https://github.com/sinai-dev/UnityExplorer) (BepInEx, il2cpp, normal (so not the CoreCLR one)) to help modding and debugging.

## 🪛Using this repository
If you download this repo to code, complete it by adding the necessary DLLs in the [RGLibs folder](https://github.com/Omglue/Omglue_RoomGirl_Mods/tree/main/src/RGLibs). If you don't know how or where, read the [README.md](https://github.com/Omglue/Omglue_RoomGirl_Mods/blob/main/src/RGLibs/README.md) of the folder for instructions.