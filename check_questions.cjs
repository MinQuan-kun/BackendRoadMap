const axios = require('axios');
const BASE_URL = 'http://localhost:7111/api';
const ADMIN_CREDENTIALS = { email: 'Admin', password: 'admin123456' };

// Correct Root IDs
const ROOTS = {
  M1: "69c3a8ae8d9c45338d958d9a", // C#
  M2: "69c3a8ae8d9c45338d958d9c", // Unity Core
  M3: "69c3a8ae8d9c45338d958dba", // Visuals
  M4: "69c3a8ae8d9c45338d958dc5", // Advanced
  M5: "69c3a8ae8d9c45338d958dd3"  // Publish
};

async function run() {
  try {
    console.log('Logging in...');
    const loginRes = await axios.post(`${BASE_URL}/users/login`, ADMIN_CREDENTIALS);
    const token = loginRes.data.token;
    const headers = { Authorization: `Bearer ${token}` };

    console.log('Fetching current questions...');
    const questionsRes = await axios.get(`${BASE_URL}/quiz/questions`);
    const questions = questionsRes.data;

    for (const q of questions) {
      let changed = false;
      for (const opt of q.options) {
        const oldNodes = opt.mappingNodes || [];
        const newNodes = [];

        if (opt.text.includes("Beginner")) newNodes.push(ROOTS.M1);
        if (opt.text.includes("Intermediate")) newNodes.push(ROOTS.M2);
        if (opt.text.includes("Advanced")) newNodes.push(ROOTS.M4);
        if (opt.text.includes("Unity (C#)")) newNodes.push(ROOTS.M2);
        if (opt.text.includes("Game 2D")) newNodes.push(ROOTS.M3);
        if (opt.text.includes("Game 3D")) newNodes.push(ROOTS.M3);
        if (opt.text.includes("Gameplay Systems")) newNodes.push(ROOTS.M4);
        if (opt.text.includes("Graphics")) newNodes.push(ROOTS.M3);
        if (opt.text.includes("Technical Leadership")) newNodes.push(ROOTS.M5);
        if (opt.text.includes("Solo Indie")) newNodes.push(ROOTS.M5);
        if (opt.text.includes("code logic")) newNodes.push(ROOTS.M1);

        if (newNodes.length > 0) {
          opt.mappingNodes = newNodes;
          changed = true;
        }
      }

      if (changed) {
        console.log(`Updating question: ${q.questionText}`);
      }
    }
    
    console.log('Update logic finished (simulation).');
    console.log('Since there is no public API to update questions individually, I will provide the updated JSON file.');

  } catch (error) {
    console.error('Error:', error.response?.data || error.message);
  }
}

run();
