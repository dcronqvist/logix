#pragma once

#include "drawables/drawable_component.hpp"
#include "integrated/ic_desc.hpp"
#include "minimals/minimal_switch.hpp"
#include "minimals/minimal_lamp.hpp"
#include <vector>
#include <string>

class DrawableIC : public DrawableComponent {
    public:
    ICDesc description;
    std::vector<CircuitComponent*> components;
    std::map<std::string, MinimalSwitch*> inputMap;
    std::vector<std::string> inputIds;
    std::map<std::string, MinimalLamp*> outputMap;
    std::vector<std::string> outputIds;

    DrawableIC(Vector2 pos, ICDesc desc) : DrawableComponent(pos, 60.0F, "", desc.inputs, desc.outputs) {
        this->description = desc;
        this->components = desc.GenerateComponents();
        this->inputMap = GetInputMap(this->components);
        this->inputIds = GetInputIDs(this->components);
        this->outputMap = GetOutputMap(this->components);
        this->outputIds = GetOutputIDs(this->components);
    }

    std::map<std::string, MinimalSwitch*> GetInputMap(std::vector<CircuitComponent*> comps) {
        std::map<std::string, MinimalSwitch*> im = {};

        for (int i = 0; i < comps.size(); i++) {
            MinimalSwitch* ms = dynamic_cast<MinimalSwitch*>(comps.at(i));
            if (ms != NULL) {
                im.insert(std::pair<std::string, MinimalSwitch*>(ms->id, ms));
            }
        }

        return im;
    }

    std::vector<std::string> GetInputIDs(std::vector<CircuitComponent*> comps) {
        std::vector<std::string> im = {};

        for (int i = 0; i < comps.size(); i++) {
            MinimalSwitch* ms = dynamic_cast<MinimalSwitch*>(comps.at(i));
            if (ms != NULL) {
                im.push_back(ms->id);
            }
        }

        return im;
    }

    std::map<std::string, MinimalLamp*> GetOutputMap(std::vector<CircuitComponent*> comps) {
        std::map<std::string, MinimalLamp*> im = {};

        for (int i = 0; i < comps.size(); i++) {
            MinimalLamp* ms = dynamic_cast<MinimalLamp*>(comps.at(i));
            if (ms != NULL) {
                im.insert(std::pair<std::string, MinimalLamp*>(ms->id, ms));
            }
        }

        return im;
    }

    std::vector<std::string> GetOutputIDs(std::vector<CircuitComponent*> comps) {
        std::vector<std::string> im = {};

        for (int i = 0; i < comps.size(); i++) {
            MinimalLamp* ms = dynamic_cast<MinimalLamp*>(comps.at(i));
            if (ms != NULL) {
                im.push_back(ms->id);
            }
        }

        return im;
    }

    void PerformLogic();

    void DrawInputs(Vector2 mousePosInWorld) {
        for (int i = 0; i < this->inputs.size(); i++) {
            CircuitInput* inp = this->inputs.at(i);
            Vector2 pos = GetInputPosition(i);
            DrawLineEx(pos, pos + Vector2{ 10.0F, 0 }, 1.5F, WHITE);

            if (inp->bits == 1) {
                Color col = (inp->GetValues().at(0)) == LogicValue_HIGH ? BLUE : WHITE;

                if (Vector2Length(pos - mousePosInWorld) < RADIUS) {
                    col = ORANGE;
                }

                DrawCircleV(pos, RADIUS, col);
            }
            else {
                Color col = BLUE * inp->GetHIGHFraction();

                if (Vector2Length(pos - mousePosInWorld) < RADIUS) {
                    col = ORANGE;
                }

                DrawCircleV(pos, RADIUS, WHITE);
                DrawCircleV(pos, RADIUS, col);

                const char* bitText = std::to_string(inp->bits).c_str();
                Vector2 textMeasure = MeasureTextEx(GetFontDefault(), bitText, 10.0F, 1.0F);
                DrawTextEx(GetFontDefault(), bitText, pos - textMeasure / 2.0F, 10.0F, 1.0F, BLACK);
            }

            const char* inputId = this->inputIds.at(i).c_str();
            Vector2 inputIdTextMeasure = MeasureTextEx(GetFontDefault(), inputId, 10.0F, 1.0F);
            DrawTextEx(GetFontDefault(), inputId, pos + Vector2{ 15.0F, 0 } - inputIdTextMeasure / 2.0F, 10.0F, 1.0F, BLACK);
        }
    }

    void DrawOutputs(Vector2 mousePosInWorld) {
        for (int i = 0; i < this->outputs.size(); i++) {
            CircuitOutput* out = this->outputs.at(i);
            Vector2 pos = GetOutputPosition(i);
            DrawLineEx(pos, pos - Vector2{ 10.0F, 0 }, 1.5F, WHITE);

            if (out->bits == 1) {
                Color col = out->GetValues().at(0) == LogicValue_HIGH ? BLUE : WHITE;

                if (Vector2Length(pos - mousePosInWorld) < RADIUS) {
                    col = ORANGE;
                }

                DrawCircleV(pos, RADIUS, col);
            }
            else {
                Color col = BLUE * out->GetHIGHFraction();

                if (Vector2Length(pos - mousePosInWorld) < RADIUS) {
                    col = ORANGE;
                }

                DrawCircleV(pos, RADIUS, WHITE);
                DrawCircleV(pos, RADIUS, col);

                const char* bitText = std::to_string(out->bits).c_str();
                Vector2 textMeasure = MeasureTextEx(GetFontDefault(), bitText, 10.0F, 1.0F);
                DrawTextEx(GetFontDefault(), bitText, pos - textMeasure / 2.0F, 10.0F, 1.0F, BLACK);
            }

            const char* outputId = this->outputIds.at(i).c_str();
            Vector2 outputIdMeasure = MeasureTextEx(GetFontDefault(), outputId, 10.0F, 1.0F);
            DrawTextEx(GetFontDefault(), outputId, pos - Vector2{ 15.0F + outputIdMeasure.x, outputIdMeasure.y / 2.0F }, 10.0F, 1.0F, BLACK);
        }
    }

    void Draw(Vector2 mousePosInWorld) {
        UpdateBox();
        DrawRectanglePro(box, Vector2{ 0.0F, 0.0F }, 0.0F, WHITE);
        DrawInputs(mousePosInWorld);
        DrawOutputs(mousePosInWorld);

        //float fontSize = 12.0F;
        //Vector2 middleOfBox = Vector2{ box.width / 2.0F, box.height / 2.0F };
        //Vector2 textSize = MeasureTextEx(GetFontDefault(), text, fontSize, 1.0F);
        //DrawTextEx(GetFontDefault(), this->text, this->position + middleOfBox - (textSize / 2.0F), fontSize, 1.0F, BLACK);
    }
};