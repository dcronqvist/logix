#pragma once

#include <map>
#include <string>
#include "utils/json.hpp"
using json = nlohmann::json;

class Settings {
    public:
    std::map<std::string, std::string> settings;

    Settings();
    std::string GetSetting(std::string key);
    void SetSetting(std::string key, std::string value);
};

void to_json(json& j, const Settings& p);
void from_json(const json& j, Settings& p);