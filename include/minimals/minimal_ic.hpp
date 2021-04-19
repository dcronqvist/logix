#pragma once

#include "circuits/circuit_component.hpp"
#include "integrated/ic_desc.hpp"
#include "minimals/minimal_switch.hpp"
#include "minimals/minimal_lamp.hpp"
#include "integrated/ic_input.hpp"
#include "integrated/ic_output.hpp"
#include <vector>
#include <string>

class MinimalIC : public CircuitComponent {
public:
    ICDesc description;
    std::vector<CircuitComponent*> components;

    MinimalIC(ICDesc desc) : CircuitComponent(std::vector<CircuitInput*>{}, std::vector<CircuitOutput*>{}) {
        this->description = desc;
        this->components = desc.GenerateComponents();
        this->inputs = desc.GenerateICInputs(this->components);
        this->outputs = desc.GenerateICOutputs(this->components);
    }

    std::vector<std::string> GetInputIDs() {
        std::vector<std::string> ids = {};
        for (int i = 0; i < this->inputs.size(); i++) {
            ids.push_back(dynamic_cast<ICInput*>(this->inputs.at(i))->id);
        }
        return ids;
    }

    std::vector<std::string> GetOutputIDs() {
        std::vector<std::string> ids = {};
        for (int i = 0; i < this->outputs.size(); i++) {
            ids.push_back(dynamic_cast<ICOutput*>(this->outputs.at(i))->id);
        }
        return ids;
    }

    void PerformLogic() {
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
    }
};