const fs = require('fs');
const nodes = JSON.parse(fs.readFileSync('GameDevRoadmapDB.Nodes.json', 'utf8'));

const moduleRoots = [1, 19, 31, 43, 55];
const roots = {};

nodes.forEach(n => {
    const name = n.name || '';
    const match = name.match(/Node (\d+)/);
    if (match) {
        const num = parseInt(match[1]);
        if (moduleRoots.includes(num)) {
            roots[num] = n._id.$oid;
        }
    }
});

console.log('Module Roots:', JSON.stringify(roots, null, 2));
