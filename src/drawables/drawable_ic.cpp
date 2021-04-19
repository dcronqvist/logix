#include "drawables/drawable_ic.hpp"
#include "raylib-cpp/raylib-cpp.hpp"

void DrawableIC::PerformLogic() {

    if (IsKeyDown(KEY_H)) {
        int x = 2;
    }

    for (int i = 0; i < this->inputs.size(); i++) {
        ICInput* ici = dynamic_cast<ICInput*>(this->inputs.at(i));

        for (int j = 0; j < ici->switchMap.size(); j++) {
            BitPointer bp = ici->switchMap.at(j);
            bp.sw->SetValue(bp.toBit, ici->GetValue(bp.fromBit));
        }
    }

    for (int i = 0; i < this->components.size(); i++) {
        this->components.at(i)->UpdateInputsAndPerformLogic();
    }

    for (int i = 0; i < this->components.size(); i++) {
        this->components.at(i)->UpdateOutputs();
    }

    for (int i = 0; i < this->outputs.size(); i++) {
        std::vector<LogicValue> values = {};
        ICOutput* ico = dynamic_cast<ICOutput*>(this->outputs.at(i));

        for (int j = 0; j < ico->lampMap.size(); j++) {
            values.push_back(ico->lampMap.at(j)->value);
        }

        ico->SetValues(values);
    }


    /*
    for (int i = 0; i < this->outputIds.size(); i++) {
        std::string id = this->outputIds.at(i);
        MinimalLamp* ms = this->outputMap.at(id);

        this->outputs.at(i)->SetValues(ms->value);
    }*/
}