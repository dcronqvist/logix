#include "projects/workspace_desc.hpp"

WorkspaceDesc::WorkspaceDesc() {
    this->components = {};
}

WorkspaceDesc::WorkspaceDesc(std::vector<DrawableComponent*> comps) {
    this->components = this->GenerateComponentDescriptions(comps);
}

int GetIndexOfDrawable(std::vector<DrawableComponent*> comps, DrawableComponent* comp) {
    // Should return index of comp in comps,
    // if not found, return -1
    auto it = std::find(comps.begin(), comps.end(), comp);
    if (it == comps.end()) { return -1; }

    return it - comps.begin();
}

std::vector<WorkspaceCompDesc> WorkspaceDesc::GenerateComponentDescriptions(std::vector<DrawableComponent*> comps) {
    std::vector<WorkspaceCompDesc> workspaceComps = {};

    for (int i = 0; i < comps.size(); i++) {
        DrawableComponent* dc = comps.at(i);

        std::string type = "";
        std::string id = "";
        int ioBits = 0;
        ICDesc* desc = NULL;

        if (dynamic_cast<DrawableGate*>(dc) != NULL) { // It is a drawablegate
            DrawableGate* gate = dynamic_cast<DrawableGate*>(dc);
            type = gate->logic->GetLogicName();
        }
        else if (dynamic_cast<DrawableSwitch*>(dc) != NULL) {
            type = "Switch";
            DrawableSwitch* ds = dynamic_cast<DrawableSwitch*>(dc);
            id = ds->id->c_str();
            ioBits = ds->bits;
        }
        else if (dynamic_cast<DrawableLamp*>(dc) != NULL) {
            type = "Lamp";
            DrawableLamp* dl = dynamic_cast<DrawableLamp*>(dc);
            id = dl->id->c_str();
            ioBits = dl->bits;
        }
        else if (dynamic_cast<DrawableHexViewer*>(dc) != NULL) {
            type = "HexViewer";
            DrawableHexViewer* dhw = dynamic_cast<DrawableHexViewer*>(dc);
            ioBits = dhw->bits;
        }
        else if (dynamic_cast<DrawableButton*>(dc) != NULL) {
            type = "Button";
        }
        else if (dynamic_cast<DrawableIC*>(dc) != NULL) {
            type = "IC";
            DrawableIC* dic = dynamic_cast<DrawableIC*>(dc);
            desc = &(dic->description);
        }
        else {
            continue;
        }

        // Do connections
        std::vector<WorkspaceConnDesc> connections = {};

        for (int j = 0; j < dc->outputs.size(); j++) {
            CircuitOutput* co = dc->outputs.at(j);

            for (int k = 0; k < co->GetSignals().size(); k++) {
                DrawableWire* dw = dynamic_cast<DrawableWire*>(co->GetSignals().at(k));
                int connectedToIndex = GetIndexOfDrawable(comps, dw->to);

                WorkspaceConnDesc wcd = { connectedToIndex, dw->fromIndex, dw->toIndex, dw->bits };
                connections.push_back(wcd);
            }
        }

        WorkspaceCompDesc compDesc = { type, dc->position, connections, id, ioBits, *(dc->GetInputVector()), desc };
        workspaceComps.push_back(compDesc);
    }

    return workspaceComps;
}

std::tuple<std::vector<DrawableComponent*>, std::vector<DrawableWire*>> WorkspaceDesc::GenerateDrawables() {
    std::vector<DrawableComponent*> comps;
    std::vector<DrawableWire*> wires;

    for (int i = 0; i < this->components.size(); i++) {
        WorkspaceCompDesc wcd = this->components.at(i);

        DrawableComponent* dc;

        if (wcd.type == "Switch") {
            DrawableSwitch* ds = new DrawableSwitch(wcd.position, wcd.ioBits);
            *(ds->id) = wcd.id;
            dc = ds;
        }
        else if (wcd.type == "Lamp") {
            DrawableLamp* ds = new DrawableLamp(wcd.position, wcd.ioBits);
            *(ds->id) = wcd.id;
            dc = ds;
        }
        else if (wcd.type == "HexViewer") {
            dc = new DrawableHexViewer(wcd.ioBits, wcd.position, &wcd.inps);
        }
        else if (wcd.type == "Button") {
            dc = new DrawableButton(wcd.position);
        }
        else if (wcd.type == "IC") {
            dc = new DrawableIC(wcd.position, *wcd.desc);
        }
        else {
            dc = new DrawableGate(wcd.position, GetGateLogic(wcd.type.c_str()), &wcd.inps);
        }

        comps.push_back(dc);
    }

    for (int i = 0; i < this->components.size(); i++) {
        WorkspaceCompDesc wcd = this->components.at(i);

        for (int j = 0; j < wcd.connectedTo.size(); j++) {
            WorkspaceConnDesc wconn = wcd.connectedTo.at(j);
            DrawableWire* dw = new DrawableWire(comps.at(i), wconn.fromOutputIndex, comps.at(wconn.to), wconn.toInputIndex, wconn.bits);

            comps.at(i)->AddOutputWire(wconn.fromOutputIndex, dw);
            comps.at(wconn.to)->SetInputWire(wconn.toInputIndex, dw);

            wires.push_back(dw);
        }
    }


    return std::tuple<std::vector<DrawableComponent*>, std::vector<DrawableWire*>>{comps, wires};
}



void to_json(json& j, const WorkspaceDesc& p) {
    j = json{
        {"components", p.components}
    };
}

void from_json(const json& j, WorkspaceDesc& p) {
    j.at("components").get_to(p.components);
}