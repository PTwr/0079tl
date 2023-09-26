// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <string>
#include <iostream>
#include <fstream>
#include <vector>

BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

#include "Plugins/PluginInterface.h";

const char* messageStackName = "R79JAF_subtitles";
constexpr u32 CYAN = 0xFF00FFFF;
constexpr u32 GREEN = 0xFF00FF00;
constexpr u32 RED = 0xFFFF0000;
constexpr u32 YELLOW = 0xFFFFFF30;


auto read_file(std::string path, bool throwOnMissingFile = false) -> std::string
{
    constexpr auto read_size = std::size_t(4096);
    auto stream = std::ifstream(path.data());
    stream.exceptions(std::ios_base::badbit);

    if (not stream)
    {
        return "";
    }

    auto out = std::string();
    auto buf = std::string(read_size, '\0');
    while (stream.read(&buf[0], read_size))
    {
        out.append(buf, 0, stream.gcount());
    }
    out.append(buf, 0, stream.gcount());
    return out;
}

struct TranslationEntry
{
    std::string Filename;
    std::string Text;
    u32 Miliseconds;
    u32 Color;
    bool Enabled;
    bool AllowDuplicate;

public:
    TranslationEntry() : Filename(""), Text(""), Miliseconds(0), Color(0), Enabled(false), AllowDuplicate(false)
    {

    }
    TranslationEntry(std::string filename, std::string text, u32 miliseconds, u32 color, bool enabled, bool allowDuplicates)
        : Filename(filename), Text(text), Miliseconds(miliseconds), Color(color), Enabled(enabled), AllowDuplicate(allowDuplicates)
    {

    }
};

class Events : public Plugins::IPluginInterface_Events
{
private:
    Plugins::IAPI* api;
    bool isInitialized;
    std::map<std::string, TranslationEntry> Translations;
public:
    Events(Plugins::IAPI* api) : IPluginInterface_Events(), api(api), isInitialized(false)
    {

    }

    void Initialize(std::string path)
    {
        auto json = read_file(path);

        if (json == "")
        {
            return;
        }
        picojson::value v;
        std::string err = picojson::parse(v, json);
        if (!err.empty())
        {
            std::cerr << err << std::endl;
            api->AddMessage("json error!", 2000, GREEN, 0, false);
            return;
        }

        if (!v.is<picojson::array>())
        {
            api->AddMessage("not an array", 2000, GREEN, 0, false);
            return;
        }

        auto arr = v.get<picojson::array>();
        for (auto item : arr)
        {
            auto FileName = item.get("FileName");
            auto Translation = item.get("Translation");
            auto Miliseconds = item.get("Miliseconds");
            auto Color = item.get("Color");
            auto Enabled = item.get("Enabled");
            auto AllowDuplicate = item.get("AllowDuplicate");

            if (!FileName.is<std::string>() ||
                !Translation.is<std::string>() ||
                !Miliseconds.is<double>() ||
                !Color.is<double>() ||
                !Enabled.is<bool>() ||
                !AllowDuplicate.is<bool>())
            {
                continue;
            }

            auto tl = TranslationEntry(FileName.to_str(), Translation.to_str(), Miliseconds.get<double>(), Color.get<double>(), Enabled.get<bool>(), AllowDuplicate.get<bool>());

            if (tl.Enabled) {
                Translations[tl.Filename] = tl;
            }
        }

        api->AddOSDMessageStack(0, 0, OSD::MessageStackDirection::Upward, true, true, messageStackName);

        isInitialized = true;
    }
    virtual void OnGameLoad(const char* gamePath) override
    {
        std::string gameId = std::string(api->GetGameId());
        api->AddMessage(gameId.c_str(), 2000, RED, 0, false);
        if (gameId == "R79JAF")
        {
            api->AddMessage("Staring subtitles for R79JAF game!", 2000, GREEN, 0, false);
            Initialize(std::string(gamePath) + ".translation.json");
        }
        else
        {
            isInitialized = false;
            api->AddMessage("Not R79JAF game!", 2000, RED, 0, false);
        }
    };
    virtual void OnGameClose(const char* gamePath) override
    {
        std::string gameId = std::string(api->GetGameId());
        api->AddMessage(gameId.c_str(), 2000, RED, 0, false);
        if (gameId == "R79JAF")
        {
            api->AddMessage("Staring subtitles for R79JAF game!", 2000, GREEN, 0, false);
            Initialize(std::string(gamePath) + ".translation.json");
        }
        else
        {
            isInitialized = false;
            api->AddMessage("Not R79JAF game!", 2000, RED, 0, false);
        }
    };
    virtual void OnFileAccess(const char* path) override
    {
        if (!isInitialized)
        {
            return;
        }

        auto key = std::string(path);
        if (Translations.contains(key))
        {
            auto tl = Translations[key];
            api->AddMessage(tl.Text.c_str(), tl.Miliseconds, tl.Color, messageStackName, !tl.AllowDuplicate);
        }
    };
};

Events* instance = 0;
extern "C"
__declspec(dllexport)
Plugins::IPluginInterface_Events * __stdcall GetPluginInterface_Events(Plugins::IAPI * api)
{
    if (!instance)
    {
        instance = new Events(api);
    }
    return instance;
}