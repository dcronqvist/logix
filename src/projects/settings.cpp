#include "projects/settings.hpp"

Settings::Settings() {
    this->settings = {};
}

void Settings::SetSetting(std::string key, std::string value) {
    this->settings.at(key) = value;
}

std::string Settings::GetSetting(std::string key) {
    return this->settings.at(key);
}

void to_json(json& j, const Settings& p) {
    j = json{
        { "settings", p.settings }
    };
}

void from_json(const json& j, Settings& p) {
    j.at("settings").get_to(p.settings);
}