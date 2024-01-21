﻿#include "types.hpp"

namespace vengine {
namespace assets {
String VEngineAssetHeader::GetSerializeId() {
  return "ASSET HEADER";
}

void VEngineAssetHeader::ReadFrom(Buffer &store) {
  store >> version;
  store >> type;
  store >> name;
  store >> meta;
}

void VEngineAssetHeader::WriteTo(Buffer &store) {
  store << version;
  store << type;
  store << name;
  store << meta;
}
}
}