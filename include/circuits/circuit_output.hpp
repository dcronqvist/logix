#pragma once
#include "circuits/circuit_wire.hpp"
#include "circuits/circuit_io.hpp"
#include <vector>
#include <algorithm>

class CircuitOutput : public CircuitIO {
    private:
    std::vector<CircuitWire*> signals;

    public:
    CircuitOutput(int bits, LogicValue initialValue) : CircuitIO(bits, initialValue) { signals = {}; }
    void AddOutputSignal(CircuitWire* wire) { signals.push_back(wire); }
    void RemoveOutputSignal(int index) { signals.erase(signals.begin() + index); }
    std::vector<CircuitWire*> GetSignals() { return this->signals; }
    bool HasAnySignal() { return signals.size() != 0; }
    void SetSignals() {
        for (int i = 0; i < signals.size(); i++) {
            signals[i]->SetValues(this->GetValues());
        }
    }
};