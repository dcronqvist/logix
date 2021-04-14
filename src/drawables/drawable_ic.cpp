#include "drawables/drawable_ic.hpp"

void DrawableIC::PerformLogic() {
    for (int i = 0; i < this->inputIds.size(); i++) {
        std::string id = this->inputIds.at(i);
        MinimalSwitch* ms = this->inputMap.at(id);

        ms->SetValues(this->inputs.at(i)->GetValues());
    }

    for (int i = 0; i < this->components.size(); i++) {
        this->components.at(i)->UpdateInputsAndPerformLogic();
    }

    for (int i = 0; i < this->components.size(); i++) {
        this->components.at(i)->UpdateOutputs();
    }

    for (int i = 0; i < this->outputIds.size(); i++) {
        std::string id = this->outputIds.at(i);
        MinimalLamp* ms = this->outputMap.at(id);

        this->outputs.at(i)->SetValues(ms->value);
    }
}